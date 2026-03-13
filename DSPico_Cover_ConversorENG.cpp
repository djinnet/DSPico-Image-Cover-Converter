#ifndef WIN32_LEAN_AND_MEAN
#define WIN32_LEAN_AND_MEAN
#endif
#include <windows.h>
#include <objidl.h>
#include <commctrl.h>
#include <shlobj.h>
#include <shlwapi.h>
#include <shellapi.h>
#include <mmsystem.h>
#include <shobjidl.h> 
#include <string>
#include <vector>
#include <algorithm>
#include <gdiplus.h>

#pragma comment(lib, "comctl32.lib")
#pragma comment(lib, "winmm.lib")
#pragma comment(lib, "gdiplus.lib")
#pragma comment(lib, "shlwapi.lib")
#pragma comment(lib, "ole32.lib")
#pragma comment(lib, "uuid.lib")

using namespace Gdiplus;

#define ID_BTN_START 1001
#define ID_BTN_CLOSE 1002
#define ID_BTN_STOP  1003
#define ID_BTN_SRC    2001
#define ID_BTN_DST    2002
#define ID_BTN_ROMS   2003
#define ID_CHK_FORCE 3001
#define ID_CHK_OPEN  3002
#define ID_CHK_CLEAN 3003

HWND hMain, hSrc, hDst, hRoms, hBtnStart, hBtnRoms, hBtnStop, hBtnClose, hChk1, hChk2, hChk3;
HBRUSH hBrBg, hBrBlue, hBrRed, hBrDark, hBrEdit, hBrOrange, hBrGray;
HFONT hFontSmall, hFontBigEdit, hFontTitle;
std::wstring currentPreview = L"dspico-logo.png";
std::vector<std::wstring> debugLog; 
float progressPercent = 0.0f;
int nImg = 0, nDone = 0, nSkipped = 0, nCleaned = 0, lastIndex = 0;
bool isWorking = false, isAbort = false, shouldExit = false;

const std::wstring txtFullTitle = L"DSPico Cover Converter v9.1 - Rikisoft 2026";

void PlayRes(const char* name) { PlaySoundA(name, GetModuleHandle(NULL), SND_RESOURCE | SND_ASYNC | SND_NODEFAULT); }

void AddLog(std::wstring msg) { 
    if(debugLog.size() > 5) debugLog.erase(debugLog.begin()); 
    debugLog.push_back(msg); InvalidateRect(hMain, NULL, FALSE); 
}

std::string GetAppPath() {
    char path[MAX_PATH]; GetModuleFileNameA(NULL, path, MAX_PATH);
    PathRemoveFileSpecA(path); return std::string(path);
}

std::string GetIniPath() { return GetAppPath() + "\\config.ini"; }

void SaveConfig() {
    std::string ini = GetIniPath();
    char s[MAX_PATH], d[MAX_PATH], r[MAX_PATH];
    GetWindowTextA(hSrc, s, MAX_PATH); GetWindowTextA(hDst, d, MAX_PATH); GetWindowTextA(hRoms, r, MAX_PATH);
    WritePrivateProfileStringA("Config", "Src", s, ini.c_str());
    WritePrivateProfileStringA("Config", "Dst", d, ini.c_str());
    WritePrivateProfileStringA("Config", "Roms", r, ini.c_str());
}

void LoadConfig() {
    std::string ini = GetIniPath();
    char s[MAX_PATH], d[MAX_PATH], r[MAX_PATH];
    GetPrivateProfileStringA("Config", "Src", "", s, MAX_PATH, ini.c_str());
    GetPrivateProfileStringA("Config", "Dst", "", d, MAX_PATH, ini.c_str());
    GetPrivateProfileStringA("Config", "Roms", "", r, MAX_PATH, ini.c_str());
    SetWindowTextA(hSrc, s); SetWindowTextA(hDst, d); SetWindowTextA(hRoms, r);
}

std::string SelectDirModern(HWND h) {
    std::string path = "";
    IFileOpenDialog *pfd = NULL;
    if (SUCCEEDED(CoCreateInstance(CLSID_FileOpenDialog, NULL, CLSCTX_INPROC_SERVER, IID_PPV_ARGS(&pfd)))) {
        DWORD dwOptions; pfd->GetOptions(&dwOptions);
        pfd->SetOptions(dwOptions | FOS_PICKFOLDERS | FOS_FORCEFILESYSTEM);
        if (SUCCEEDED(pfd->Show(h))) {
            IShellItem *psi = NULL;
            if (SUCCEEDED(pfd->GetResult(&psi))) {
                PWSTR pszPath = NULL;
                if (SUCCEEDED(psi->GetDisplayName(SIGDN_FILESYSPATH, &pszPath))) {
                    char buf[MAX_PATH]; WideCharToMultiByte(CP_UTF8, 0, pszPath, -1, buf, MAX_PATH, NULL, NULL);
                    path = buf; CoTaskMemFree(pszPath); PlayRes("SND_SELECT");
                }
                psi->Release();
            }
        } else PlayRes("SND_QUITAR");
        pfd->Release();
    }
    return path;
}

void EjecutarLimpieza(std::string pathDst, std::string pathRoms) {
    WIN32_FIND_DATAA ffd;
    HANDLE hFind = FindFirstFileA((pathDst + "\\*.bmp").c_str(), &ffd);
    if (hFind == INVALID_HANDLE_VALUE) return;
    do {
        std::string bmpName = ffd.cFileName;
        size_t lastDot = bmpName.find_last_of(".");
        if (lastDot != std::string::npos) {
            std::string romBase = bmpName.substr(0, lastDot);
            if (!PathFileExistsA((pathRoms + "\\" + romBase).c_str())) {
                if (DeleteFileA((pathDst + "\\" + bmpName).c_str())) nCleaned++;
            }
        }
    } while (FindNextFileA(hFind, &ffd));
    FindClose(hFind);
}

DWORD WINAPI ThreadMotor(LPVOID lpParam) {
    char cSrc[MAX_PATH], cDst[MAX_PATH];
    GetWindowTextA(hSrc, cSrc, MAX_PATH); GetWindowTextA(hDst, cDst, MAX_PATH);
    std::vector<std::string> lista; WIN32_FIND_DATAA ffd;
    HANDLE hFind = FindFirstFileA((std::string(cSrc) + "\\*.*").c_str(), &ffd);
    if (hFind != INVALID_HANDLE_VALUE) {
        do {
            std::string n = ffd.cFileName; if(n == "." || n == "..") continue;
            size_t dot = n.find_last_of(".");
            if (dot != std::string::npos) {
                std::string ext = n.substr(dot + 1); std::transform(ext.begin(), ext.end(), ext.begin(), ::tolower);
                if (ext == "png" || ext == "jpg" || ext == "jpeg") lista.push_back(n);
            }
        } while (FindNextFileA(hFind, &ffd)); FindClose(hFind);
    }
    for (size_t i = lastIndex; i < lista.size(); ++i) {
        if (isAbort) { lastIndex = (int)i; break; }
        std::string out = std::string(cDst) + "\\" + lista[i].substr(0, lista[i].find_last_of(".")) + ".bmp";
        if (PathFileExistsA(out.c_str()) && SendMessage(hChk1, BM_GETCHECK, 0, 0) != BST_CHECKED) { nSkipped++; continue; }
        nDone++; progressPercent = (float)(nSkipped + nDone) / nImg;
        std::string fullPath = std::string(cSrc) + "\\" + lista[i];
        currentPreview = std::wstring(fullPath.begin(), fullPath.end());
        InvalidateRect(hMain, NULL, FALSE);
        std::string cmd = "magick.exe \"" + fullPath + "\" -resize 106x96! -background black -gravity northwest -extent 128x96 -type Palette -depth 8 -colors 256 -compress none \"BMP3:" + out + "\"";
        STARTUPINFOA si = { sizeof(si) }; PROCESS_INFORMATION pi;
        if (CreateProcessA(NULL, (char*)cmd.c_str(), NULL, NULL, FALSE, CREATE_NO_WINDOW, NULL, NULL, &si, &pi)) {
            WaitForSingleObject(pi.hProcess, INFINITE); CloseHandle(pi.hProcess); CloseHandle(pi.hThread);
        }
    }
    isWorking = false;
    if (shouldExit) { SaveConfig(); PostMessage(hMain, WM_CLOSE, 0, 0); }
    else if (isAbort) {
        SetWindowTextW(hBtnStop, L"RESUME"); EnableWindow(hBtnStop, TRUE);
        AddLog(L"Left: " + std::to_wstring(nImg - (nDone + nSkipped))); SetWindowTextW(hBtnStart, L"RESTART");
    } else { 
        lastIndex = 0; EnableWindow(hBtnStop, FALSE); SetWindowTextW(hBtnStop, L"STOP"); 
        AddLog(L"All done!"); PlayRes("SND_FINISH"); SetWindowTextW(hBtnStart, L"START");
        if(SendMessage(hChk2, BM_GETCHECK, 0, 0) == BST_CHECKED) ShellExecuteA(NULL, "open", cDst, NULL, NULL, SW_SHOW);
    }
    currentPreview = L"dspico-logo.png"; InvalidateRect(hMain, NULL, FALSE); return 0;
}

LRESULT CALLBACK WndProc(HWND hwnd, UINT msg, WPARAM wp, LPARAM lp) {
    switch (msg) {
        case WM_NCHITTEST: return HTCAPTION;
        case WM_ERASEBKGND: return 1;
        case WM_CTLCOLORSTATIC: SetTextColor((HDC)wp, RGB(120, 120, 120)); SetBkColor((HDC)wp, RGB(15, 15, 20)); return (LRESULT)hBrBg;
        case WM_CTLCOLOREDIT: SetTextColor((HDC)wp, RGB(220, 220, 220)); SetBkColor((HDC)wp, RGB(35, 35, 38)); return (LRESULT)hBrEdit;
        case WM_DRAWITEM: {
            LPDRAWITEMSTRUCT p = (LPDRAWITEMSTRUCT)lp; HBRUSH br = NULL;
            if (p->CtlID == ID_BTN_STOP) {
                if (isWorking || (isAbort && lastIndex > 0)) {
                    FillRect(p->hDC, &p->rcItem, hBrDark);
                    RECT rcP = p->rcItem; rcP.right = (int)(rcP.left + (p->rcItem.right - p->rcItem.left) * progressPercent);
                    HBRUSH hB = isWorking ? CreateSolidBrush(RGB(0, 190, 90)) : hBrGray;
                    FillRect(p->hDC, &rcP, hB); if(isWorking) DeleteObject(hB);
                    FrameRect(p->hDC, &p->rcItem, hBrOrange);
                    wchar_t pctT[64]; GetWindowTextW(p->hwndItem, pctT, 64);
                    std::wstring fT = std::to_wstring((int)(progressPercent * 100)) + L"% - " + pctT;
                    SetTextColor(p->hDC, RGB(255, 255, 255)); SetBkMode(p->hDC, TRANSPARENT);
                    SelectObject(p->hDC, hFontTitle); DrawTextW(p->hDC, fT.c_str(), -1, &p->rcItem, DT_CENTER|DT_VCENTER|DT_SINGLELINE);
                    return TRUE;
                } else br = IsWindowEnabled(p->hwndItem) ? hBrOrange : hBrDark;
            } else {
                if (p->CtlID == ID_BTN_START) br = hBrBlue;
                else if (p->CtlID == ID_BTN_CLOSE) br = hBrRed;
                else br = hBrDark;
            }
            if (!IsWindowEnabled(p->hwndItem)) br = hBrDark;
            FillRect(p->hDC, &p->rcItem, br);
            SetTextColor(p->hDC, IsWindowEnabled(p->hwndItem) ? RGB(255, 255, 255) : RGB(80, 80, 80));
            SetBkMode(p->hDC, TRANSPARENT); SelectObject(p->hDC, hFontSmall);
            wchar_t t[64]; GetWindowTextW(p->hwndItem, t, 64);
            DrawTextW(p->hDC, t, -1, &p->rcItem, DT_CENTER|DT_VCENTER|DT_SINGLELINE);
            return TRUE;
        }
        case WM_PAINT: {
            PAINTSTRUCT ps; HDC hdc = BeginPaint(hwnd, &ps); RECT rc; GetClientRect(hwnd, &rc);
            HDC hdcMem = CreateCompatibleDC(hdc); HBITMAP hbmMem = CreateCompatibleBitmap(hdc, rc.right, rc.bottom);
            SelectObject(hdcMem, hbmMem); FillRect(hdcMem, &rc, hBrBg);
            Graphics g(hdcMem); g.SetSmoothingMode(SmoothingModeHighQuality);
            Font fTitle(hdcMem, hFontTitle); SolidBrush bN(Color(255, 0, 220, 255));
            RectF rT; g.MeasureString(txtFullTitle.c_str(), -1, &fTitle, PointF(0,0), &rT);
            g.DrawString(txtFullTitle.c_str(), -1, &fTitle, PointF((rc.right - rT.Width)/2, 15), &bN);
            SelectObject(hdcMem, hFontSmall); SetBkMode(hdcMem, TRANSPARENT); SetTextColor(hdcMem, RGB(120, 120, 120));
            TextOutW(hdcMem, 25, 40, L"SOURCE COVERS", 13); TextOutW(hdcMem, 25, 83, L"DSPICO (SDPico_pico\\covers\\user)", 32);
            TextOutW(hdcMem, 25, 126, L"ROMS (SDPico\\Roms Folder)", 25);
            int midY = 245; wchar_t sT[32], sH[32], sS[32], sC[32];
            swprintf(sT, 32, L"Total: %d", nImg); swprintf(sH, 32, L"Done: %d", nDone); swprintf(sS, 32, L"Skipped: %d", nSkipped); swprintf(sC, 32, L"Clean: %d", nCleaned);
            COLORREF cOff = RGB(80, 80, 80);
            SetTextColor(hdcMem, nImg > 0 ? RGB(100, 180, 100) : cOff); RECT rt1={25, midY-30, 150, midY-15}; DrawTextW(hdcMem, sT, -1, &rt1, DT_LEFT);
            SetTextColor(hdcMem, nDone > 0 ? RGB(0, 180, 255) : cOff); RECT rt2={25, midY-15, 150, midY}; DrawTextW(hdcMem, sH, -1, &rt2, DT_LEFT);
            SetTextColor(hdcMem, nSkipped > 0 ? RGB(240, 240, 100) : cOff); RECT rt3={25, midY, 150, midY+15}; DrawTextW(hdcMem, sS, -1, &rt3, DT_LEFT);
            SetTextColor(hdcMem, nCleaned > 0 ? RGB(255, 140, 0) : cOff); RECT rt4={25, midY+15, 150, midY+30}; DrawTextW(hdcMem, sC, -1, &rt4, DT_LEFT);
            if (PathFileExistsW(currentPreview.c_str())) { Image img(currentPreview.c_str()); g.DrawImage(&img, 157, 220, 95, 95); }
            for(size_t i=0; i<debugLog.size(); ++i) {
                SetTextColor(hdcMem, debugLog[i].find(L"ERR:") != std::wstring::npos ? RGB(255,80,80) : RGB(0,150,255));
                RECT rd={rc.right-180, (int)(midY-30 + i*15), rc.right-25, (int)(midY-15 + i*15)}; DrawTextW(hdcMem, debugLog[i].c_str(), -1, &rd, DT_RIGHT);
            }
            BitBlt(hdc, 0, 0, rc.right, rc.bottom, hdcMem, 0, 0, SRCCOPY);
            DeleteObject(hbmMem); DeleteDC(hdcMem); EndPaint(hwnd, &ps); return 0;
        }
        case WM_COMMAND:
            if (LOWORD(wp) == ID_BTN_START) {
                if (isWorking) return 0;
                char cS[MAX_PATH], cD[MAX_PATH], cR[MAX_PATH];
                GetWindowTextA(hSrc, cS, MAX_PATH); GetWindowTextA(hDst, cD, MAX_PATH); GetWindowTextA(hRoms, cR, MAX_PATH);
                if (!PathFileExistsA(cS) || !PathFileExistsA(cD) || strlen(cS) < 3) { debugLog.clear(); AddLog(L"ERR: Paths"); PlayRes("SND_QUITAR"); return 0; }
                std::vector<std::string> lista; WIN32_FIND_DATAA ffd; HANDLE hF = FindFirstFileA((std::string(cS) + "\\*.*").c_str(), &ffd);
                if (hF != INVALID_HANDLE_VALUE) {
                    do { std::string n = ffd.cFileName; if(n == "." || n == "..") continue;
                        size_t d = n.find_last_of("."); if (d != std::string::npos) {
                            std::string e = n.substr(d+1); std::transform(e.begin(), e.end(), e.begin(), ::tolower);
                            if (e=="png"||e=="jpg"||e=="jpeg") lista.push_back(n);
                        }
                    } while (FindNextFileA(hF, &ffd)); FindClose(hF);
                }
                nImg = (int)lista.size(); if (nImg == 0) { debugLog.clear(); AddLog(L"Source empty"); return 0; }
                int yaEx = 0; bool force = (SendMessage(hChk1, BM_GETCHECK, 0, 0) == BST_CHECKED); bool limp = (SendMessage(hChk3, BM_GETCHECK, 0, 0) == BST_CHECKED);
                for(auto& img : lista) { std::string o = std::string(cD) + "\\" + img.substr(0, img.find_last_of(".")) + ".bmp"; if (PathFileExistsA(o.c_str())) yaEx++; }
                if (!force && !limp && yaEx == nImg) { nSkipped = yaEx; nDone = 0; debugLog.clear(); AddLog(L"Up to date!"); PlayRes("SND_SELECT"); SetWindowTextW(hBtnStart, L"START"); return 0; }
                debugLog.clear(); nCleaned = 0; nDone = 0; nSkipped = 0; lastIndex = 0; isAbort = false; isWorking = true; shouldExit = false;
                if (limp) { if (strlen(cR) >= 3 && PathFileExistsA(cR)) EjecutarLimpieza(cD, cR); else { AddLog(L"ERR: Rom path"); isWorking = false; return 0; } }
                SetWindowTextW(hBtnStart, L"START"); SetWindowTextW(hBtnStop, L"STOP"); EnableWindow(hBtnStop, TRUE);
                AddLog(L"Working..."); CreateThread(0,0,ThreadMotor,0,0,0);
            }
            if (LOWORD(wp) == ID_BTN_STOP) {
                debugLog.clear();
                if(isWorking) { isAbort = true; SetWindowTextW(hBtnStart, L"RESTART"); }
                else if (isAbort && lastIndex > 0) { 
                    isAbort = false; isWorking = true; AddLog(L"Left: " + std::to_wstring(nImg - (nDone + nSkipped)));
                    SetWindowTextW(hBtnStart, L"RESTART"); SetWindowTextW(hBtnStop, L"STOP"); CreateThread(0,0,ThreadMotor,0,0,0); 
                }
            }
            if (LOWORD(wp) == ID_BTN_CLOSE) { 
                if (isWorking) { 
                    shouldExit = true; isAbort = true; AddLog(L"Closing..."); 
                    EnableWindow(hBtnClose, FALSE); EnableWindow(hBtnStart, FALSE); EnableWindow(hBtnStop, FALSE); 
                } else { SaveConfig(); PostMessage(hwnd, WM_CLOSE, 0, 0); } 
            }
            if (LOWORD(wp) >= 2001 && LOWORD(wp) <= 2003) {
                PlayRes("SND_SELECT");
                std::string d = SelectDirModern(hwnd); if(!d.empty()){
                    if(LOWORD(wp)==2001) SetWindowTextA(hSrc, d.c_str()); if(LOWORD(wp)==2002) SetWindowTextA(hDst, d.c_str()); if(LOWORD(wp)==2003) SetWindowTextA(hRoms, d.c_str());
                }
            }
            if (LOWORD(wp) >= ID_CHK_FORCE && LOWORD(wp) <= ID_CHK_CLEAN) {
                bool isChecked = (SendMessage((HWND)lp, BM_GETCHECK, 0, 0) == BST_CHECKED);
                PlayRes(isChecked ? "SND_SELECT" : "SND_QUITAR");
                if (LOWORD(wp) == ID_CHK_CLEAN) { EnableWindow(hRoms, isChecked); EnableWindow(hBtnRoms, isChecked); }
            }
            return 0;
        case WM_CREATE: {
            hFontSmall = CreateFontW(13, 0, 0, 0, 400, 0, 0, 0, 0, 0, 0, 6, 0, L"Segoe UI");
            hFontBigEdit = CreateFontW(15, 0, 0, 0, 400, 0, 0, 0, 0, 0, 0, 6, 0, L"Segoe UI");
            hFontTitle = CreateFontW(16, 0, 0, 0, 800, 0, 0, 0, 0, 0, 0, 6, 0, L"Segoe UI");
            hSrc = CreateWindowExA(0,"EDIT","",WS_VISIBLE|WS_CHILD|WS_BORDER|ES_AUTOHSCROLL, 25, 53, 320, 22, hwnd, 0, 0, 0);
            CreateWindowW(L"BUTTON", L"...", WS_VISIBLE|WS_CHILD|BS_OWNERDRAW, 355, 53, 30, 22, hwnd, (HMENU)ID_BTN_SRC, 0, 0);
            hDst = CreateWindowExA(0,"EDIT","",WS_VISIBLE|WS_CHILD|WS_BORDER|ES_AUTOHSCROLL, 25, 96, 320, 22, hwnd, 0, 0, 0);
            CreateWindowW(L"BUTTON", L"...", WS_VISIBLE|WS_CHILD|BS_OWNERDRAW, 355, 96, 30, 22, hwnd, (HMENU)ID_BTN_DST, 0, 0);
            hRoms = CreateWindowExA(0,"EDIT","",WS_VISIBLE|WS_CHILD|WS_BORDER|WS_DISABLED|ES_AUTOHSCROLL, 25, 139, 320, 22, hwnd, 0, 0, 0);
            hBtnRoms = CreateWindowW(L"BUTTON", L"...", WS_VISIBLE|WS_CHILD|BS_OWNERDRAW|WS_DISABLED, 355, 139, 30, 22, hwnd, (HMENU)ID_BTN_ROMS, 0, 0);
            hChk1 = CreateWindowW(L"BUTTON", L"Overwrite", WS_VISIBLE|WS_CHILD|BS_AUTOCHECKBOX, 25, 175, 105, 20, hwnd, (HMENU)ID_CHK_FORCE, 0, 0);
            hChk2 = CreateWindowW(L"BUTTON", L"Open when done", WS_VISIBLE|WS_CHILD|BS_AUTOCHECKBOX, 145, 175, 125, 20, hwnd, (HMENU)ID_CHK_OPEN, 0, 0);
            hChk3 = CreateWindowW(L"BUTTON", L"Clean covers", WS_VISIBLE|WS_CHILD|BS_AUTOCHECKBOX, 295, 175, 105, 20, hwnd, (HMENU)ID_CHK_CLEAN, 0, 0);
            hBtnStart = CreateWindowW(L"BUTTON", L"START", WS_VISIBLE|WS_CHILD|BS_OWNERDRAW, 25, 340, 70, 28, hwnd, (HMENU)ID_BTN_START, 0, 0);
            hBtnStop  = CreateWindowW(L"BUTTON", L"STOP", WS_VISIBLE|WS_CHILD|BS_OWNERDRAW|WS_DISABLED, 140, 340, 130, 28, hwnd, (HMENU)ID_BTN_STOP, 0, 0);
            hBtnClose = CreateWindowW(L"BUTTON", L"CLOSE", WS_VISIBLE|WS_CHILD|BS_OWNERDRAW, 315, 340, 70, 28, hwnd, (HMENU)ID_BTN_CLOSE, 0, 0);
            SendMessage(hSrc, WM_SETFONT, (WPARAM)hFontBigEdit, 1); SendMessage(hDst, WM_SETFONT, (WPARAM)hFontBigEdit, 1);
            SendMessage(hRoms, WM_SETFONT, (WPARAM)hFontBigEdit, 1); SendMessage(hChk1, WM_SETFONT, (WPARAM)hFontSmall, 1);
            SendMessage(hChk2, WM_SETFONT, (WPARAM)hFontSmall, 1); SendMessage(hChk3, WM_SETFONT, (WPARAM)hFontSmall, 1);
            LoadConfig(); 

            // PATH VALIDATION
            char dummy[MAX_PATH];
            if (SearchPathA(NULL, "magick.exe", NULL, MAX_PATH, dummy, NULL) == 0) {
                AddLog(L"ERR: magick PATH");
                int res = MessageBoxW(hwnd, 
                    L"ImageMagick (magick.exe) was not found in your system PATH.\n\n"
                    L"Want to open the official download page?\n"
                    L"(The app will close to let you install it).", 
                    L"Tool Not Found", MB_YESNO | MB_ICONWARNING);
                
                if (res == IDYES) {
                    ShellExecuteW(NULL, L"open", L"https://imagemagick.org/script/download.php#windows", NULL, NULL, SW_SHOWNORMAL);
                    PostQuitMessage(0); 
                    return 0;
                }
                EnableWindow(hBtnStart, FALSE);
            }
            break;
        }
        case WM_CLOSE: if (isWorking) { shouldExit = true; isAbort = true; return 0; } DestroyWindow(hwnd); break;
        case WM_DESTROY: PostQuitMessage(0); break;
    } return DefWindowProc(hwnd, msg, wp, lp);
}

int WINAPI WinMain(HINSTANCE hI, HINSTANCE, LPSTR, int) {
    CoInitializeEx(NULL, COINIT_APARTMENTTHREADED); GdiplusStartupInput gsi; ULONG_PTR tok; GdiplusStartup(&tok, &gsi, NULL);
    hBrBg = CreateSolidBrush(RGB(15, 15, 20)); hBrBlue = CreateSolidBrush(RGB(0, 100, 200)); hBrRed = CreateSolidBrush(RGB(150, 30, 30));
    hBrDark = CreateSolidBrush(RGB(35, 35, 40)); hBrEdit = CreateSolidBrush(RGB(35, 35, 38)); hBrOrange = CreateSolidBrush(RGB(200, 100, 0));
    hBrGray = CreateSolidBrush(RGB(80, 80, 80));
    WNDCLASSA wc = {0}; wc.lpfnWndProc = WndProc; wc.hInstance = hI; wc.lpszClassName = "P222"; wc.hCursor = LoadCursor(0, IDC_ARROW);
    RegisterClassA(&wc);
    hMain = CreateWindowExA(0, "P222", "DSPico", WS_POPUP|WS_VISIBLE, 400, 200, 410, 393, NULL, NULL, hI, NULL);
    MSG m; while (GetMessage(&m, 0, 0, 0)) { TranslateMessage(&m); DispatchMessage(&m); }
    GdiplusShutdown(tok); CoUninitialize(); return (int)m.wParam;
}
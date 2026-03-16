# DSPico-Image-Cover-Converter

Convert almost any image to DSPico cover format.

# Instructions
To compile, use [.NET 9](https://dotnet.microsoft.com/es-es/download/dotnet/9.0) or modify the .csproj file to use the version you prefer.

You can use the following command to compile from command line or any of its variants, depending on the size and portability you want:

```bash
dotnet publish -c Release -r win-x64 --self-contained false /p:PublishSingleFile=true /p:IncludeNativeLibrariesForSelfExtract=true
```

This project were originally from [Original Project](https://github.com/Sad0wner/DSPico-Image-Conversor), 
but I have modified it to be more dev-friendly, so it can be used with a designer while maintaining the same functionality as the original.


The original project was in Spanish, so I have also translated it to English. And I have added an resource file to make it easier to translate to other languages in the future.


The same resource file also makes it easier to get the images / icons from the original project.

Credits to:
- [@Sad0wner](https://github.com/Sad0wner) for the original project.
- [@Djinnet](https://github.com/Djinnet) for the modifications and the english localization.

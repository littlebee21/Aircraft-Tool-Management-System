时间:2018年
功能: 对与往届下位机部分，添加SD卡的支持

1,对二维码照相

2,图片上传上位机

3,SD卡存储

4,屏幕显示拍摄图像

具体参考：照相机项目.mmap
关键字：单片机，stm32f103，照相，二维码识别, 串口
文档：丢失

```.
├── README.md
├── git_new.sh
├── 图像采集和下位机通讯
│   └── 实验39 照相机实验
│       ├── CORE
│       │   ├── core_cm3.c
│       │   ├── core_cm3.h
│       │   └── startup_stm32f10x_hd.s
│       ├── FATFS
│       │   ├── doc
│       │   │   ├── css_e.css
│       │   │   ├── css_j.css
│       │   │   ├── css_p.css
│       │   │   ├── en
│       │   │   ├── img
│       │   │   │   ├── app1.c
│       │   │   │   ├── app2.c
│       │   ├── exfuns
│       │   │   ├── exfuns.c
│       │   │   ├── exfuns.h
│       │   │   ├── fattester.c
│       │   │   ├── fattester.h
│       │   │   └── mycc936.c
│       │   ├── fattester.c
│       │   ├── fattester.h
│       │   └── src
│       │       ├── 00readme.txt
│       │       ├── diskio.c
│       │       ├── diskio.h
│       │       ├── ff.c
│       │       ├── ff.h
│       │       ├── ffconf.h
│       │       ├── history.txt
│       │       ├── integer.h
│       │       └── option
│       │           ├── cc932.c
│       │           ├── cc936.c
│       │           ├── cc949.c
│       │           ├── cc950.c
│       │           ├── ccsbcs.c
│       │           ├── mycc936.c
│       │           ├── syscall.c
│       │           └── unicode.c
│       ├── HARDWARE
│       │   ├── BEEP
│       │   │   ├── beep.c
│       │   │   └── beep.h
│       │   ├── EXTI
│       │   │   ├── exti.c
│       │   │   └── exti.h
│       │   ├── KEY
│       │   │   ├── key.c
│       │   │   └── key.h
│       │   ├── LCD
│       │   │   ├── font.h
│       │   │   ├── lcd.c
│       │   │   └── lcd.h
│       │   ├── LED
│       │   │   ├── led.c
│       │   │   └── led.h
│       │   ├── OV7670
│       │   │   ├── ov7670.c
│       │   │   ├── ov7670.h
│       │   │   ├── ov7670cfg.h
│       │   │   ├── sccb.c
│       │   │   └── sccb.h
│       │   ├── SDIO
│       │   │   ├── sdio_sdcard.c
│       │   │   └── sdio_sdcard.h
│       │   ├── SPI
│       │   │   ├── spi.c
│       │   │   └── spi.h
│       │   ├── TIMER
│       │   │   ├── timer.c
│       │   │   └── timer.h
│       │   ├── TPAD
│       │   │   ├── tpad.c
│       │   │   └── tpad.h
│       │   └── W25QXX
│       │       ├── w25qxx.c
│       │       └── w25qxx.h
│       ├── MALLOC
│       │   ├── malloc.c
│       │   └── malloc.h
│       ├── OBJ
│       │   ├── w25qxx.crf
│       │   ├── w25qxx.d
│       │   └── w25qxx.o
│       ├── PICTURE
│       │   ├── bmp.c
│       │   ├── bmp.h
│       │   ├── gif.c
│       │   ├── gif.h
│       │   ├── integer.h
│       │   ├── piclib.c
│       │   ├── piclib.h
│       │   ├── tjpgd.c
│       │   └── tjpgd.h
│       ├── README.TXT
│       ├── STM32F10x_FWLib
│       │   ├── inc
│       │   │   ├── misc.h
│       │   │   ├── stm32f10x_adc.h
│       │   │   ├── stm32f10x_bkp.h
│       │   │   ├── stm32f10x_can.h
│       │   │   ├── stm32f10x_cec.h
│       │   │   ├── stm32f10x_crc.h
│       │   │   ├── stm32f10x_dac.h
│       │   │   ├── stm32f10x_dbgmcu.h
│       │   │   ├── stm32f10x_dma.h
│       │   │   ├── stm32f10x_exti.h
│       │   │   ├── stm32f10x_flash.h
│       │   │   ├── stm32f10x_fsmc.h
│       │   │   ├── stm32f10x_gpio.h
│       │   │   ├── stm32f10x_i2c.h
│       │   │   ├── stm32f10x_iwdg.h
│       │   │   ├── stm32f10x_pwr.h
│       │   │   ├── stm32f10x_rcc.h
│       │   │   ├── stm32f10x_rtc.h
│       │   │   ├── stm32f10x_sdio.h
│       │   │   ├── stm32f10x_spi.h
│       │   │   ├── stm32f10x_tim.h
│       │   │   ├── stm32f10x_usart.h
│       │   │   └── stm32f10x_wwdg.h
│       │   └── src
│       │       ├── misc.c
│       │       ├── stm32f10x_adc.c
│       │       ├── stm32f10x_bkp.c
│       │       ├── stm32f10x_can.c
│       │       ├── stm32f10x_cec.c
│       │       ├── stm32f10x_crc.c
│       │       ├── stm32f10x_dac.c
│       │       ├── stm32f10x_dbgmcu.c
│       │       ├── stm32f10x_dma.c
│       │       ├── stm32f10x_exti.c
│       │       ├── stm32f10x_flash.c
│       │       ├── stm32f10x_fsmc.c
│       │       ├── stm32f10x_gpio.c
│       │       ├── stm32f10x_i2c.c
│       │       ├── stm32f10x_iwdg.c
│       │       ├── stm32f10x_pwr.c
│       │       ├── stm32f10x_rcc.c
│       │       ├── stm32f10x_rtc.c
│       │       ├── stm32f10x_sdio.c
│       │       ├── stm32f10x_spi.c
│       │       ├── stm32f10x_tim.c
│       │       ├── stm32f10x_usart.c
│       │       └── stm32f10x_wwdg.c
│       ├── SYSTEM
│       │   ├── delay
│       │   │   ├── delay.c
│       │   │   └── delay.h
│       │   ├── sys
│       │   │   ├── sys.c
│       │   │   └── sys.h
│       │   └── usart
│       │       ├── usart.c
│       │       └── usart.h
│       ├── TEXT
│       │   ├── fontupd.c
│       │   ├── fontupd.h
│       │   ├── text.c
│       │   └── text.h
│       ├── USER
│       │   ├── CAMERA.map
│       │   ├── CAMERA.uvguix.Administrator
│       │   ├── CAMERA.uvoptx
│       │   ├── CAMERA.uvprojx
│       │   ├── DebugConfig
│       │   │   └── CAMERA_STM32F103ZE_1.0.0.dbgconf
│       │   ├── JLinkSettings.ini
│       │   ├── main.c
│       │   ├── startup_stm32f10x_hd.lst
│       │   ├── stm32f10x.h
│       │   ├── stm32f10x_conf.h
│       │   ├── stm32f10x_it.c
│       │   ├── stm32f10x_it.h
│       │   ├── system_stm32f10x.c
│       │   ├── system_stm32f10x.h
│       │   └── 此电脑.lnk
│       ├── USMART
│       │   ├── readme.txt
│       │   ├── usmart.c
│       │   ├── usmart.h
│       │   ├── usmart_config.c
│       │   ├── usmart_str.c
│       │   └── usmart_str.h
│       ├── keilkilll.bat
│       └── 照相机项目.mmap
├── 工具管理.jpg
├── 飞机维修工具
│   ├── 飞机维修工具.mdf
│   └── 飞机维修工具_log.ldf
└── 高级项目
    ├── Form1.Designer.cs
    ├── Form1.cs
    ├── Form1.resx
    ├── Program.cs
    ├── Properties
    │   ├── AssemblyInfo.cs
    │   ├── Resources.Designer.cs
    │   ├── Resources.resx
    │   ├── Settings.Designer.cs
    │   └── Settings.settings
    ├── Resources
    ├── bin
    │   ├── Debug
    │   │   ├── E01-条形码.vshost.exe.manifest
    │   │   ├── EAN_13-9787302380979.jpg
    │   │   ├── QR-9787302380979.jpg
    │   │   ├── logo.jpg
    │   │   ├── zxing.dll
    │   │   ├── zxing.pdb
    │   │   ├── zxing.xml
    │   │   ├── 高级项目.exe
    │   │   ├── 高级项目.pdb
    │   │   ├── 高级项目.vshost.exe
    │   │   └── 高级项目.vshost.exe.manifest
    │   └── Release
    ├── obj
    │   └── x86
    │       └── Debug
    │           ├── DesignTimeResolveAssemblyReferences.cache
    │           ├── DesignTimeResolveAssemblyReferencesInput.cache
    │           ├── E01-条形码.csproj.FileListAbsolute.txt
    │           ├── E01-条形码.csproj.GenerateResource.Cache
    │           ├── E01-条形码.csprojResolveAssemblyReference.cache
    │           ├── E01_条形码.timebox1.resources
    │           ├── TempPE
    │           │   └── Properties.Resources.Designer.cs.dll
    │           ├── 高级项目.Properties.Resources.resources
    │           ├── 高级项目.csproj.CopyComplete
    │           ├── 高级项目.csproj.CoreCompileInputs.cache
    │           ├── 高级项目.csproj.FileListAbsolute.txt
    │           ├── 高级项目.csproj.GenerateResource.cache
    │           ├── 高级项目.csprojAssemblyReference.cache
    │           ├── 高级项目.csprojResolveAssemblyReference.cache
    │           ├── 高级项目.exe
    │           └── 高级项目.pdb
    ├── 高级项目.csproj
    ├── 高级项目.csproj.user
    └── 高级项目.sln

48 directories, 353 files
```

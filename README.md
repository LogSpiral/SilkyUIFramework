<div align="center">
    <img src="icon_workshop.png" width="200"/>
    <h3>SilkyUI Framework</h3>
</div>

### 发展

**SilkyUI** 原来只是 **更好的体验** 与 **积分商店** 两个模组内使用的对于 **Terraria.UI** 的一些扩展与更改。\
由于原来是模组内部直接维护 UI，所以在**积分商店**与**更好的体验**之间的代码同步极为不方便，并且随着后续的更新需求与要求逐渐高了起来，仅对 **Terraria.UI** 进行扩展便产生了局限性，故而将 UI 部分分离出来，方便多模组直接使用其进行开发，并命名为 **SilkyUI**。

### Terraria.UI 的问题

**SilkyUI** 是为了简化 UI 开发而设计的，如果你深度使用过 **Terraria.UI** 那么你会发现其中很多的不便，例如：UI 之间没有统一的管理，需要自己维护、没有专业布局功能、布局依赖直接设定元素的位置来进行布局、没有自适应大小、且布局更新需要主动调用 **Recalculate()** 来触发、布局仅能使用纯 C# 代码形式，没有声明式布局支持等诸多问题。

### SilkyUI 的解决方案

以上问题不仅会产生非常多的冗余代码，还会导致开发体验很差，同时编写的代码也难以维护。\
**SilkyUI** 中，当你想要创建一个 UI 只需要继承 **BasicBody** 类，添加 **RegisterUI** 特性便能直接创建一个游戏中可直接使用的 UI，并且 UI 之间有层级关系，不会出现鼠标穿透问题，且能类似 **Windows 窗口** 一样根据你所操作的 UI 更新层级关系。\
**UIView & UIElementGroup** 中对布局会产生影响的属性，都会在更新是进行脏标记，无需主动调用 **Recalculate()** 计算布局的同时还能进行局部更新。\
**SilkyUI** 现已实现了 [CSS Flexible Box Layout Module Level 1](https://www.w3.org/TR/css-flexbox-1/) 标准的 **Flexbox 布局** 功能，并且 Grid 也在计划中，后续也会慢慢实现。\
**SilkyUI** 支持使用 **XML** 进行布局，**XML** 中可以 **复用样式** 和 **生成元素映射**。

### 相关文档

[Flexbox 文档](FlexboxModule.md)

[CSS Flexible Box Layout Module Level 1](https://www.w3.org/TR/css-flexbox-1/)

### TODO

1. [ ] Grid 布局设计与实现

### XML 初始模板：

```xml
<?xml version="1.0" encoding="utf-8" ?>
<!-- Class 填写对应类名 -->
<Body Class="SilkyUIFramework.UserInterfaces.MouseMenuUI">
    <!-- ElementGroup 是基本元素 -->
    <!-- Width 和 Height 的类型实现了 IParsable<TSelf>，演示的写法是支持的方式 -->
    <ElementGroup Width="100px 0%" Height="100%">
        <!-- 这是内置的文本元素，目前仅有简单支持，使用泰拉的 TextSnippet -->
        <TextView Text="Hello SilkyUI"/>
    </ElementGroup>
</Body>
```

### XML 属性转 C#对象 属性和字段赋值，支持的属性和字段类型:

1. 实现 IParsable<TSelf> 的任意类，会调用 Parse 方法进行解析。
2. float double char int string
3. Vector2 Vector3 Vector4 Color

### 如何使用？

SilkyUI Framework 是以前置 Mod 的方式在 tML 中使用的（为了不同 UI 之前的协同），所以需要将其设置为前置 Mod。

> 由于使用了**增量生成器**，所以你还需要一同将生成器项目引入你的 Mod 项目。

### 设为前置 Mod

在主 Mod 项目中的 build.txt 文件内添加模组引用

```txt
modReferences = SilkyUIFramework
```

### 克隆必要项目

在你的 ModSources 文件夹下运行以下两个 git clone 命令

```sh
git clone https://github.com/487666123/SilkyUIFramework.git
git clone https://github.com/487666123/SilkyUIAnalyzer.git
```

### 引入这两个项目

1. 将两个项目添加入你的解决方案中（选中 .csproj 文件）
![alt text](image.png)
2. 这两个项目会出现在你的解决方案中
![alt text](image-1.png)
3. 在你的项目的 .csporj 文件中添加这两个项目的引用
```xml
<ItemGroup>
    <ProjectReference Include="..\SilkyUIAnalyzer\SilkyUIAnalyzer.csproj">
        <OutputItemType>Analyzer</OutputItemType>
        <ReferenceOutputAssembly>false</ReferenceOutputAssembly>
    </ProjectReference>
    <ProjectReference Include="..\SilkyUIFramework\SilkyUIFramework.csproj"></ProjectReference>
</ItemGroup>
```

<hr/>

<div align=center>
    <h3>豆包生成的几个 LOGO</h3>
    <img src="logo1.png" width="200"/>
    <img src="logo3.jpg" width="200"/>
    <img src="logo2.jpg" width="200"/>
</div>
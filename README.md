<div align="center">
    <h1>Silky User Interface Framework</h1>
    <img src="icon_workshop.png" width="200"/>
    <h3>SilkyUI Framework</h3>
</div>

<hr/>

<div align=center>
    <h3>豆包生成的几个 LOGO</h3>
    <img src="logo1.png" width="200"/>
    <img src="logo3.jpg" width="200"/>
    <img src="logo2.jpg" width="200"/>
</div>

### 说明

[Flexbox 文档](FlexibleBoxModule.md)

[CSS Flexible Box Layout Module Level 1](https://www.w3.org/TR/css-flexbox-1/)

### 待办

1. [x] Flexbox 完善
2. [x] FlexWrap 实现
3. [x] position: sticky 实现
4. [ ] Grid 布局设计与实现
5. [x] 模糊效果实现

XML 初始模板：
```xml
<?xml version="1.0" encoding="utf-8" ?>
<!-- Class 填写对应类名 -->
<Body Class="SilkyUIFramework.UserInterfaces.MouseMenuUI">
</Body>
```

支持的类:

1. 实现 IParsable<TSelf> 的任意类，会调用 Parse 方法进行解析。
2. float double char int string
3. Vector2 Vector3 Vector4 Color
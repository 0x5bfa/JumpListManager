<h1 align="center">Jump List Viewer</h1>

<p align="center">A modern WinUI 3 tool for Windows 10/11 that lets you view and analyze Jump Lists from applications.<br/>With this tool, you can visually inspect and debug those lists with ease.</p>

## ✨ Features

_Coming soon._

## 🚀 Usage

1. Prerequisits
    - Windows 10  (Build 10.0.17763.0) onwards and Windws 11
    - Visual Studio 2022 with `slnx` enabled
    - .NET 9 SDK
2. Clone the repo
    ```console
    git clone https://github.com/0x5bfa/JumpListViewer.git
    ```
3. Open `JumpListViewer.slnx`
4. Build the solution

## 🖼 Screenshot

<img alt="screenshot1" src="https://github.com/user-attachments/assets/991b8640-9cb7-4f96-89c3-bf44a2b2993f" />

## 🧠 Usage

```C#
var jumpList = JumpList.Default;

foreach (var category in jumpList.GetItems())
{
    Console.WriteLine($"Category: {category.Key}");

    foreach (var item in category)
    {
        Console.WriteLine($"- {item.Name}");
    }
}
```

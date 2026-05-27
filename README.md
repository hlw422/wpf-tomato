# 🍅 WPF 番茄时钟 (WPF Pomodoro Timer)

一个基于 WPF 和 MVVM 架构的现代化番茄工作法计时器应用，帮助你提高工作效率。

## ✨ 功能特性

### 🎯 核心计时功能
- **番茄工作法循环**：25分钟专注 → 5分钟短休息 → 重复4次后15分钟长休息
- **可视化倒计时**：圆形进度环实时显示剩余时间
- **模式指示器**：清晰显示当前状态（专注/短休息/长休息）
- **任务内容输入**：记录当前专注的具体任务

### ⚙️ 自定义设置
- 专注时长（默认25分钟）
- 短休息时长（默认5分钟）
- 长休息时长（默认15分钟）
- 长休息间隔（默认每4个番茄）
- 自动开始休息/专注
- 最小化到系统托盘
- 提示音开关
- 桌面通知开关

### 📊 数据统计
- 今日完成番茄数
- 今日专注时长
- 每日历史记录
- 每周统计汇总
- 本地 JSON 持久化存储

### 🖥️ 系统集成
- **系统托盘**：最小化后常驻托盘，支持气泡通知
- **全局热键**：无需切换窗口即可控制计时器
  - `Ctrl+Alt+T`：开始/暂停
  - `Ctrl+Alt+R`：重置
  - `Ctrl+Alt+S`：跳过
- **桌面通知**：计时完成时弹出系统通知

### 🎨 界面设计
- 现代简洁的浅色主题
- 橙色为主色调，专注模式下突出显示
- 圆角卡片式布局
- 响应式按钮状态

## 📸 界面预览

```
┌─────────────────────────────────────┐
│  🍅 番茄时钟    [计时器] [统计] [设置] │
├─────────────────────────────────────┤
│                                     │
│           ╭─────────╮              │
│           │  专注    │              │
│           ╰─────────╯              │
│                                     │
│           ╭─────────╮              │
│           │  25:00  │              │
│           │  就绪   │              │
│           ╰─────────╯              │
│                                     │
│        番茄进度: 0/4 | 今日: 0个    │
│                                     │
│      [ 输入当前任务内容... ]         │
│                                     │
│    [▶ 开始]  [↺ 重置]  [⏭ 跳过]    │
│                                     │
│   Ctrl+Alt+T 暂停 | R 重置 | S 跳过 │
└─────────────────────────────────────┘
```

## 🚀 快速开始

### 环境要求
- Windows 10/11
- .NET 10.0 SDK

### 安装运行

#### 方式一：直接运行
1. 下载最新 Release 版本
2. 解压后双击 `Wpf_Tomato.exe` 运行

#### 方式二：从源码构建
```bash
# 克隆仓库
git clone https://github.com/hlw422/wpf-tomato.git
cd wpf-tomato

# 构建项目
dotnet build

# 运行应用
dotnet run
```

#### 方式三：发布独立版本
```bash
# 发布为独立可执行文件（无需安装 .NET 运行时）
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```

## 📁 项目结构

```
Wpf_Tomato/
├── App.xaml                    # 应用程序入口配置
├── App.xaml.cs                 # 应用程序生命周期管理
├── MainWindow.xaml             # 主窗口界面定义
├── MainWindow.xaml.cs          # 主窗口代码
├── Wpf_Tomato.csproj           # 项目文件
├── app.ico                     # 应用图标
│
├── Models/                     # 数据模型
│   ├── AppData.cs              # 应用数据根对象
│   ├── DailyStatistics.cs      # 每日统计模型
│   ├── PomodoroRecord.cs       # 番茄记录模型
│   └── PomodoroSettings.cs     # 设置模型
│
├── Services/                   # 业务服务
│   ├── DataPersistenceService.cs  # 数据持久化服务
│   ├── HotKeyService.cs        # 全局热键服务
│   ├── PomodoroTimerService.cs # 计时器核心服务
│   ├── SoundService.cs         # 声音通知服务
│   └── TrayService.cs          # 系统托盘服务
│
├── ViewModels/                 # 视图模型
│   └── MainViewModel.cs        # 主视图模型（MVVM核心）
│
└── Converters/                 # 值转换器
    ├── BoolToVisibilityConverter.cs    # 布尔值转可见性
    ├── ProgressOffsetConverter.cs      # 进度环偏移转换
    ├── ProgressToColorConverter.cs     # 进度转颜色转换
    └── ProgressWidthConverter.cs       # 进度宽度转换
```

## 🛠️ 技术栈

| 技术 | 说明 |
|------|------|
| **框架** | .NET 10.0 (WPF) |
| **架构模式** | MVVM (Model-View-ViewModel) |
| **MVVM库** | CommunityToolkit.Mvvm 8.4.0 |
| **数据存储** | JSON 文件 (System.Text.Json) |
| **系统集成** | Win32 API (RegisterHotKey, NotifyIcon) |
| **UI框架** | WPF XAML |

## 🎹 快捷键

| 快捷键 | 功能 |
|--------|------|
| `Ctrl+Alt+T` | 开始/暂停计时 |
| `Ctrl+Alt+R` | 重置当前计时 |
| `Ctrl+Alt+S` | 跳过当前阶段 |

## 📝 使用说明

### 基本操作
1. **开始专注**：点击「开始」按钮或按 `Ctrl+Alt+T`
2. **暂停计时**：再次点击「暂停」或按 `Ctrl+Alt+T`
3. **重置计时**：点击「重置」或按 `Ctrl+Alt+R`
4. **跳过阶段**：点击「跳过」或按 `Ctrl+Alt+S`

### 任务记录
- 在输入框中输入当前要专注的任务内容
- 任务内容会随计时器状态显示

### 查看统计
- 切换到「统计」标签页查看历史数据
- 显示每日完成的番茄数和专注时长

### 个性化设置
- 切换到「设置」标签页自定义各项参数
- 点击「保存设置」应用更改

## 💾 数据存储

应用数据存储在用户目录下：
```
%AppData%/Wpf_Tomato/data.json
```

数据结构：
```json
{
  "Settings": {
    "FocusDurationMinutes": 25,
    "ShortBreakDurationMinutes": 5,
    "LongBreakDurationMinutes": 15,
    "LongBreakInterval": 4,
    "AutoStartBreaks": false,
    "AutoStartPomodoros": false,
    "MinimizeToTray": true,
    "PlaySound": true,
    "ShowNotification": true
  },
  "Records": [
    {
      "StartTime": "2026-05-27T09:00:00",
      "EndTime": "2026-05-27T09:25:00",
      "FocusDurationMinutes": 25,
      "Completed": true
    }
  ]
}
```

## 🤝 贡献指南

欢迎提交 Issue 和 Pull Request！

1. Fork 本仓库
2. 创建功能分支 (`git checkout -b feature/AmazingFeature`)
3. 提交更改 (`git commit -m 'Add some AmazingFeature'`)
4. 推送到分支 (`git push origin feature/AmazingFeature`)
5. 创建 Pull Request

## 📄 许可证

本项目采用 MIT 许可证 - 详见 [LICENSE](LICENSE) 文件

## 🙏 致谢

- [CommunityToolkit.Mvvm](https://github.com/CommunityToolkit/dotnet) - MVVM 框架
- [番茄工作法](https://en.wikipedia.org/wiki/Pomodoro_Technique) - 时间管理方法论

---

**专注当下，高效工作！** 🍅

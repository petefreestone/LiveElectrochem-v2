# Live Electrochem v2

**Live Electrochem v2** is a WPF-based application for visualizing and analyzing electrochemical data recorded in LabVIEW-generated binary formats from the WCCV software). It supports high-resolution voltammetry data processing, interactive GUI controls, and real-time display of signal features such as charge (FSCAV) and current.

---

## 🚀 Features

- Load and parse `.bin` files with custom binary structure
- Display:
  - Voltammogram (raw and filtered)
  - Integrated charge vs time
  - Sweep navigation and selection
- Real-time filtering (Butterworth, user-configurable)
- Background subtraction
- Calibration routines and RMS analysis
- Export tools for:
  - Charge
  - Voltammogram
  - Triangle waveform
  - Background response
  - Temporal plots
- Rich UI built with:
  - Material Design in XAML
  - OxyPlot for scientific plotting
  - Extended WPF Toolkit for property grid

---

## 🛠 Technologies Used

- .NET 8
- WPF
- C#
- OxyPlot
- MaterialDesignThemes
- Extended.Wpf.Toolkit
- Custom LabVIEW `.vi` readers

---

## ⚙️ Getting Started

1. Clone the repo
2. Open `Live Electrochem v2.sln` in Visual Studio 2022 or later
3. Build the solution (make sure `.NET 8 SDK` is installed)
4. Run the application

---

## Publications

[Subthalamic nucleus exclusively evokes dopamine release in the tail of the striatum](https://onlinelibrary.wiley.com/doi/full/10.1111/jnc.15677)

Kathryn L. Todd, Janusz Lipski, Peter S. Freestone

[Dopamine transmission in the tail striatum: Regional variation and contribution of dopamine clearance mechanisms](https://onlinelibrary.wiley.com/doi/10.1111/jnc.16052)

Bronwyn Riley, Emily Gould, Jordan Lloyd, Luke E. Hallum, Srdjan Vlajkovic, Kathryn Todd, Peter S. Freestone


---

## 🔐 License

This project is licensed under the MIT License. See [LICENSE](LICENSE) for more details.

---

## 📧 Contact

For licensing or extension requests, contact: **Peter Freestone**  
📧 `peter.s.freestone@gmail.com`

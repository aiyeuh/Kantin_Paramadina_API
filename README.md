# Kantin_Paramadina_API

# 🍽️ Kantin Paramadina API
Backend REST API untuk sistem Kantin Paramadina, dibangun menggunakan **ASP.NET Core** dengan arsitektur bersih, mendukung autentikasi, CRUD data, upload file, serta integrasi dengan database SQL Server.

---

## 📘 Daftar Isi
- [Deskripsi](#-deskripsi)
- [Teknologi yang Digunakan](#-teknologi-yang-digunakan)
- [Fitur Utama](#-fitur-utama)
- [Struktur Folder](#-struktur-folder)
- [Instalasi](#-instalasi)
- [Konfigurasi](#-konfigurasi)
- [Menjalankan Proyek](#-menjalankan-proyek)
- [Dokumentasi Endpoint (Contoh)](#-dokumentasi-endpoint-contoh)
- [Upload File](#-upload-file)
- [Kontribusi](#-kontribusi)
- [Lisensi](#-lisensi)

---

## 📄 Deskripsi
Kantin Paramadina API adalah aplikasi backend yang menyediakan layanan API untuk aplikasi kantin kampus. API ini menangani:

- Manajemen user (Admin & Customer)
- Manajemen menu makanan/minuman
- Pemesanan & transaksi
- Upload QRIS / gambar lainnya
- Autentikasi dengan JWT (opsional, jika ditambahkan)

Proyek ini menggunakan arsitektur modular sehingga mudah untuk dikembangkan, diuji, dan di-maintain.

---

## 🛠️ Teknologi yang Digunakan
- **C# / .NET 8 (atau versi yang digunakan project)**
- **ASP.NET Core Web API**
- **Entity Framework Core**
- **SQL Server**
- **AutoMapper**
- **IFormFile upload handling**
- **Dependency Injection bawaan .NET**

---

## 🚀 Fitur Utama
- CRUD **Menu**
- CRUD **Outlet**
- CRUD **User**
- CRUD **Transaksi**
- Upload file (gambar, bukti pembayaran, QRIS)
- Mapping Model ↔ DTO menggunakan AutoMapper
- Middleware custom untuk validation / exception handling
- Static file hosting untuk file upload

---

## 📁 Struktur Folder
/Controllers # Tempat semua REST API controllers
/DTO # Data Transfer Objects (request & response)
/Data # DbContext, konfigurasi database
/Mappings # AutoMapper profile & mapping rules
/Middleware # Custom middleware (logging, exception, dsb)
/Model # Entity model untuk database
/wwwroot # Static files (upload files, images, qris)
/wwwroot/uploads/qris# Folder khusus upload QRIS
Program.cs # Entry point aplikasi
Kantin_Paramadina.csproj
Kantin_Paramadina.sln
appsettings.json # Konfigurasi default

## ⚙️ Instalasi

### 1. Clone Repository
```bash
git clone https://github.com/aiyeuh/Kantin_Paramadina_API.git
cd Kantin_Paramadina_API

# Sentinel

**Sentinel** adalah permainan penembak jitu (sniper) 3D dengan tema zombi yang dikembangkan menggunakan Unity. Dalam permainan ini, pemain berperan sebagai seorang *sniper* yang ditugaskan untuk mengeliminasi zombi. Keunikan utama dari game ini adalah kemampuannya untuk dikendalikan menggunakan **pengontrol perangkat keras khusus (custom hardware controller)** yang dilengkapi dengan sensor IMU (Inertial Measurement Unit) maupun pengontrol berbasis *web/mobile*, sehingga pemain membidik menggunakan pergerakan fisik di dunia nyata.

### Demo Gameplay

Lihat cuplikan *gameplay* di bawah ini:
https://github.com/user-attachments/assets/d57b0bca-37f0-4802-8f8a-3fd2760cba29

---

## 🎮 Fitur Utama Gameplay

*   **Pengontrol Perangkat Keras Fisik (Custom Hardware Controller)**: Pemain dapat membidik target menggunakan pergerakan nyata berkat sensor IMU yang mengirimkan data putaran (Roll, Pitch, Yaw) melalui port Serial (COM). Terdapat juga tombol fisik terpisah untuk menembak, mengisi peluru (*reload*), dan membuka bidikan (*scope*).
*   **Dukungan Pengontrol Web/Mobile (WebSocket)**: Sebagai alternatif dari Serial port, game ini menyediakan penerima data `Socket.IO` bawaan (`IMUReceiver.cs`) untuk menerima data IMU dari server Node.js lokal. Ini memungkinkan penggunaan ponsel pintar (*smartphone*) sebagai alat bidik.
*   **Mode Fallback Keyboard & Mouse**: Jika perangkat keras khusus sedang tidak tersambung atau tidak aktif, sistem otomatis mendeteksi masukan (*input*) dari mouse dan keyboard sehingga game tetap dapat dimainkan seperti game FPS pada umumnya.
*   **Efek Bullet Time Sinematik**: Terinspirasi dari game *Sniper Elite*, ketika pemain menembak musuh dari jarak tertentu, kamera akan melambat (Slow-Motion) dan secara dramatis mengikuti peluru hingga mengenai target (*Bullet Time Sequence*).
*   **Tingkat Kesulitan (Difficulty Scaling)**: Terdapat 3 opsi tingkat kesulitan:
    *   **Mudah (Easy)**: 4 zombi aktif, zombi ditempatkan di lokasi yang mudah ditemukan.
    *   **Menengah (Medium)**: 4 zombi aktif, zombi disembunyikan di lokasi-lokasi strategis.
    *   **Sulit (Hard)**: 5 zombi aktif, zombi dapat bergerak (berpatroli) dan sulit ditemukan.
*   **Manajemen Sumber Daya (Amunisi)**: Pemain memiliki batasan jumlah majalah peluru. Setiap tembakan harus diperhitungkan dengan matang. Jika peluru habis sebelum semua zombi dibersihkan, permainan berakhir (Game Over).

---

## 🛠️ Ikhtisar Teknis & Arsitektur

Game ini dibangun dengan pendekatan modular menggunakan bahasa C#. Berikut adalah penjelasan mendalam tentang *script* utama yang menggerakkan logika game:

### Sistem Inti

*   `GameManager.cs`: 
    *   Bertanggung jawab atas state (status) utama permainan, mulai dari menentukan posisi dan jumlah zombi yang di-*spawn* berdasarkan tingkat kesulitan, melacak jumlah *kill*, mengelola amunisi (sisa *reserve ammo*), hingga menampilkan layar *Win/Lose*. 
    *   *Script* ini juga mengatur musik latar belakang (BGM) ketika memenangkan atau kalah dalam permainan.
*   `ShootController.cs`: 
    *   Menangani logika penembakan, penggunaan amunisi, dan transisi ke efek *Bullet Time*.
    *   Membaca *input* dari `SerialReaderThreaded`, mengelola *raycast* (garis tembak tak kasat mata) untuk mendeteksi *hit* pada musuh, memunculkan prefab peluru visual, dan membunyikan *Sound Effects* penembakan (*SFX*).
*   `EnemyController.cs`: 
    *   Mengatur AI sederhana dari zombi, termasuk sistem patroli dari titik *waypoint* ke *waypoint* lainnya menggunakan perhitungan posisi vektor.
    *   Berinteraksi langsung dengan `RagdollController` dan `EnemyAnimationController`. Ketika zombi terkena tembakan, komponen *Ragdoll* akan diaktifkan untuk memberikan efek jatuh yang realistis berdasarkan arah gaya tembak (*physics-based impulse*).

### Sistem Pengontrol (Controller Systems)

*   `SerialReaderThreaded.cs`: 
    *   Membaca data sensor (Roll, Pitch, Yaw) dan status 3 tombol (Shoot, Reload, Scope) dari mikrokontroler (mis. Arduino/ESP32) melalui port Serial.
    *   **Multithreading**: Pembacaan data port Serial dilakukan di *thread* terpisah agar tidak menyebabkan hambatan kinerja (*lag/stutter*) pada *Main Thread* (FPS game utama tetap mulus).
*   `IMUReceiver.cs`: 
    *   Sistem alternatif berbasis jaringan (WebSocket) menggunakan `Socket.IO`. 
    *   *Script* ini menghubungkan game ke `http://localhost:3000` dan mendengarkan event bernama `"message"` untuk mem-parsing data JSON yang berisi koordinat rotasi *yaw, pitch, dan roll*.

### Efek Kamera & Sinematik

*   `BulletTimeController.cs`: 
    *   Mengontrol urutan sinematik (*cinematic sequence*) ketika peluru ditembakkan dengan mulus.
    *   Menggunakan **Cinemachine Dolly Cart & Track** untuk membuat kamera meluncur mengikuti peluru di udara.
    *   Ketika peluru hampir mengenai zombi, secara otomatis mendestruksi jalur peluru dan beralih ke jalur kamera musuh untuk memperlihatkan dampak tabrakan. Skala waktu (TimeScale) akan dimanipulasi melalui `TimeScaleController` agar terasa lambat (*slow-motion*).

### Pemetaan Input (Input Mapping)

*   **Pengontrol Fisik (Serial / IMU)**:
    *   Gerakan *Pitch/Yaw/Roll* → Mengarahkan kursor/bidikan senjata.
    *   Tombol 1 → Menembak (*Shoot*).
    *   Tombol 2 → Mengisi Ulang Peluru (*Reload*).
    *   Tombol 3 → Membuka Lensa Bidik (*Scope*).
*   **Mouse & Keyboard (Mode Fallback)**:
    *   Gerakan Mouse → Mengarahkan bidikan.
    *   Klik Kiri Mouse → Menembak (*Shoot*).
    *   Tombol `R` → Mengisi Ulang Peluru (*Reload*).

---

## 🚀 Panduan Setup & Instalasi

Ikuti langkah-langkah berikut untuk menjalankan Sentinel di komputer Anda:

1. **Unduh Repositori**: *Clone* atau *download* repository proyek game ini ke komputer Anda.
2. **Buka di Unity**: Buka proyek `Sentinel` melalui **Unity Hub**. Pastikan menggunakan versi Unity yang sesuai dengan proyek (Cek pengaturan *ProjectVersion.txt* atau jalankan versi Unity terbaru yang kompatibel dengan *package* Cinemachine).
3. **Buka Scene**: Di dalam panel `Project`, navigasikan ke *folder* `Assets` (atau `Assets/Scenes` jika ada), dan buka *scene* utama game.
4. **Konfigurasi Pengontrol Serial Fisik (Jika digunakan)**:
    * Hubungkan mikrokontroler (contoh: Arduino/ESP32 dengan modul MPU6050) ke PC.
    * Pastikan terdeteksi di `COM9`. Jika berbeda, buka *script* `SerialReaderThreaded.cs` dan ubah nilai `COM9` sesuai dengan *port* mikrokontroler Anda (misalnya `COM3`).
    * Pastikan kode pada mikrokontroler menggunakan *Baud rate* sebesar `38400` dan mengirim format data `Roll,Pitch,Yaw,Shoot,Reload,Scope`.
5. **Konfigurasi Pengontrol WebSocket (Jika digunakan)**:
    * Pastikan Anda memiliki *server* Node.js yang berjalan secara lokal di *port* `3000`.
    * *Server* harus memancarkan (emit) *event* bernama `"message"` dan memberikan kembalian JSON dengan properti `yaw`, `pitch`, dan `roll`.
6. **Mulai Permainan**: Tekan tombol **Play** (▶) di bagian atas Editor Unity.

---

## ⚙️ Cara Bermain

1. **Pilih Kesulitan**: Pada layar Menu Utama (*Main Menu*), pilih tingkat kesulitan yang Anda inginkan (Easy, Medium, Hard). Semakin sulit, posisi zombi akan semakin tersembunyi dan bergerak (patroli).
2. **Cari Target**: Pindai sekeliling area permainan dengan memutar pengontrol fisik Anda (atau *mouse*) untuk menemukan lokasi zombi yang tersebar.
3. **Eksekusi**: Bidik dengan hati-hati ke arah zombi, tekan tombol tembak. Saksikan aksi *Bullet Time* ketika tembakan jarak jauh yang presisi meluncur menghantam musuh!
4. **Manajemen Amunisi**: Selalu pantau sisa peluru Anda di pojok layar! Tekan tombol *Reload* (atau `R` di *keyboard*) saat sisa peluru di dalam magasin (senjata) habis. Anda hanya memiliki kapasitas amunisi yang terbatas.
5. **Kondisi Menang**: Eliminasi **seluruh zombi** di dalam peta sebelum amunisi Anda habis seluruhnya untuk memenangkan permainan.

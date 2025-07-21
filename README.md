# 🐄 Farm Expert API System

A complete farm management system for livestock tracking, milk production monitoring, and real-time notifications.

---

## 🧩 **About the Project**

This system provides a robust backend API built with **ASP.NET Core**, **Entity Framework Core**, and **SQL Server** following **Clean Architecture** principles.

- Developed a secure RESTful API to manage farms and staff.
- Implemented **JWT-based authentication** with **Farm ID–based authorization**.
- Full **CRUD operations** for milk production (with time segmentation: AM, Noon, PM).
- Supports **real-time notifications** via SignalR (without Firebase).
- Designed for multiple user roles:
  - **Admin**: Creates farms and assigns managers.
  - **Manager**: Manages workers and oversees the web application.
  - **Worker**: Can manage only  web application not Workers or Veterainer .
  - **Veterinarian**: Manages the vet clinic and cattle.

---

## 🌐 Live Demo

### ✅ API
🔗 [https://farmxpertapi.runasp.net/api/](https://farmxpertapi.runasp.net/api/)

> ⚠️ Note: Most API endpoints require authentication. Only the weather endpoint is public.

### 🔍 Swagger UI: 👉
🔗https://farmxpertapi.runasp.net/swagger/index.html

### ✅ Web Dashboard
🔗 [https://farm-expert-next.vercel.app/en](https://farm-expert-next.vercel.app/en)  
This is a website linked to the above API, providing a full dashboard for managing the smart farm.

#### 👥 User Roles:
- 🧑‍💼 **Admin** – Full control over users and the entire system.
- 👨‍🌾 **Farm Manager** – Manages animals, workers, and farm activities.
- 🧑‍🔧 **Farm Staff** – Limited access to daily tasks and operations.

---

### 🚀 Try the Dashboard

You can try the system using one of the demo accounts below:

| Role         | Email         | Password   |
|--------------|---------------|------------|
| Admin        | A1@gmail.com  | 1234512345 |
| Farm Manager | m2@gmail.com  | 1234512345 |
| Farm Staff   | W5@gmail.com  | 1234512345 |

> 📝 Each account has a different access level and experience tailored to their role.

---

## 👤 **User Roles & Permissions**

| Role        | Permissions                                                                 |
|-------------|------------------------------------------------------------------------------|
| **Admin**   | Add farms, add managers, manage global data                                 |
| **Manager** | Add and manage workers, view and control farm-related data and milk records |
| **Worker**  | Limited to managing other workers within the same farm                      |
| **Vet**     | Manages veterinary clinic and cattle data                                   |

---

## 📡 **API Endpoints Overview**

### 🔐 Authentication

| Endpoint                  | Method | Description              |
|---------------------------|--------|--------------------------|
| `Auth/login`             | POST   | User login               |
| `logout`                 | POST   | User logout              |
| `Auth/forgot-password`   | POST   | Send reset password link |
| `Auth/reset-password`    | POST   | Reset password           |

---

### 👷 Workers

| Endpoint                    | Method | Description         |
|-----------------------------|--------|---------------------|
| `Worker/AddWorker`         | POST   | Add new worker      |
| `Worker/all`               | GET    | Get all workers     |
| `Worker/{id}`              | GET    | Get worker by ID    |
| `Worker/UpdateWorker/{id}`| PUT    | Update worker       |
| `Worker/delete/{id}`      | DELETE | Delete worker       |

---

### 👨‍⚕️ Veterinarians

| Endpoint                            | Method | Description           |
|-------------------------------------|--------|-----------------------|
| `Veterinarians/AddVeterinar`       | POST   | Add new vet           |
| `Veterinarians/all`                | GET    | Get all vets          |
| `Veterinarians/{id}`               | GET    | Get vet by ID         |
| `Veterinarians/UpdateVeterinar/{id}`| PUT    | Update vet            |
| `Veterinarians/delete/{id}`        | DELETE | Delete vet            |

---

### 🐄 Cattle

| Endpoint                                         | Method | Description                    |
|--------------------------------------------------|--------|--------------------------------|
| `Cattle/AddCattle`                               | POST   | Add new cattle                 |
| `Cattle/DeleteCattle/{id}`                       | DELETE | Delete cattle by ID            |
| `Cattle/UpdateCattle/{id}`                       | PUT    | Update cattle by ID            |
| `Cattle/GetCattlesByType/{Type}`                 | GET    | Get cattle by type             |
| `Cattle/GetCattleByTypeAndId/{Type}/{Id}`        | GET    | Get cattle by type and ID      |

---
### 🐄 Cattle Activity – Individual Events

| Endpoint                              | Method | Description                       |
|---------------------------------------|--------|-----------------------------------|
| `CattleActivityIND/EventTypes`        | GET    | Get individual cattle event types |
| `CattleActivityIND/AddEvent`          | POST   | Add event for a specific cattle   |
| `CattleActivityIND/AllEvents`         | GET    | Get all individual events         |
| `CattleActivityIND/EditEvent/{id}`    | PUT    | Edit individual event by ID       |
| `CattleActivityIND/DeleteEvent/{id}`  | DELETE | Delete individual event by ID     |

---

### 📦 Cattle Activity – Mass Events

| Endpoint                                 | Method | Description                        |
|------------------------------------------|--------|------------------------------------|
| `CattleActivityIND/EventTypes`           | GET    | Get mass event types (shared)      |
| `CattleActivityIND/AddMassEvent`         | POST   | Add mass event                     |
| `CattleActivityIND/AllMassEvents`        | GET    | Get all mass events                |
| `CattleActivityIND/EditMassEvent/{id}`   | PUT    | Edit mass event by ID              |
| `CattleActivityIND/DeleteMassEvent/{id}` | DELETE | Delete mass event by ID            |
 
---

### 🥛 Milk Production

| Endpoint                                               | Method | Description                           |
|--------------------------------------------------------|--------|---------------------------------------|
| `MilkProduction/GetCattlesByTypeAndGender`            | GET    | Filter cattle by type & gender         |
| `MilkProduction/Add`                                  | POST   | Add milk record                        |
| `MilkProduction/AddMultiple`                          | POST   | Add multiple milk records at once      |
| `MilkProduction/Edit/{id}`                            | PUT    | Edit milk record                       |
| `MilkProduction/Delete/{id}`                          | DELETE | Delete milk record                     |
| `MilkProduction/All`                                  | GET    | Get all milk production records        |

---

### 🔔 Worker Notifications

| Endpoint                                | Method | Description                         |
|-----------------------------------------|--------|-------------------------------------|
| `Notification/custom`                   | POST   | Send custom notification            |
| `Notification/worker-notifications`     | GET    | Get notifications for worker        |
| `Notification/all-worker-notifications` | GET    | Get all worker notifications        |
| `Notification/mark-as-read/{id}`        | PUT    | Mark notification as read by ID     |
| `Notification/delete/{id}`              | DELETE | Delete notification by ID           |

---

### 🌤️ Weather Alerts

| Endpoint            | Method | Description                 |
|---------------------|--------|-----------------------------|
| `Weather/weather`   | GET    | Get current weather data    |
| `Weather/alerts`    | GET    | Get weather alerts/warnings |

--- 

### 📬 Contact Submission

| Endpoint         | Method | Description                              |
|------------------|--------|------------------------------------------|
| `Contact/Submit` | POST   | Submit contact form (name, phone, email) |

---

## 📂 Technologies Used

- **ASP.NET Core Web API**
- **Entity Framework Core**
- **SQL Server**
- **SignalR** (for real-time notifications)
- **JWT** (for secure authentication)

---

## 📌 Notes

- All APIs are secured. You must authenticate first (except for `/weather`).
- Designed using Clean Architecture to ensure scalability and separation of concerns.
- Easily extendable to support mobile or frontend integration (React, Angular, etc.)

---

## 📧 Contact

For questions or collaboration, feel free to reach out via GitHub or email:mohamedredac7@gmail.com or Phone:01151406421.


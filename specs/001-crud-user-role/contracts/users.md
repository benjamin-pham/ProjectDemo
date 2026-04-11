# Contract: User Endpoints

**Tính năng**: `001-crud-user-role` | **Base URL**: `/api/users`  
**Xác thực**: Tất cả endpoints yêu cầu `Authorization: Bearer <jwt_token>`

---

## GET /api/users — Danh sách User (có phân trang)

### Request

| Tham số | Vị trí | Kiểu | Bắt buộc | Mô tả |
|---------|--------|------|----------|-------|
| `page` | query | int | Không | Trang hiện tại (mặc định: 1, min: 1) |
| `pageSize` | query | int | Không | Số bản ghi/trang (mặc định: 20, max: 100) |

### Response 200 OK

```json
{
  "items": [
    {
      "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "firstName": "Nguyen",
      "lastName": "Van A",
      "username": "nguyenvana",
      "email": "a@example.com",
      "phone": "0901234567",
      "birthday": "1990-05-15",
      "createdAt": "2026-04-01T08:00:00Z"
    }
  ],
  "totalCount": 42,
  "page": 1,
  "pageSize": 20
}
```

**Lưu ý**: `PasswordHash`, `HashedRefreshToken` không bao giờ xuất hiện trong response.

### Response 401 Unauthorized

Khi thiếu hoặc token không hợp lệ.

---

## GET /api/users/{id} — Chi tiết User

### Request

| Tham số | Vị trí | Kiểu | Bắt buộc | Mô tả |
|---------|--------|------|----------|-------|
| `id` | path | Guid | Có | ID của User |

### Response 200 OK

```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "firstName": "Nguyen",
  "lastName": "Van A",
  "username": "nguyenvana",
  "email": "a@example.com",
  "phone": "0901234567",
  "birthday": "1990-05-15",
  "createdAt": "2026-04-01T08:00:00Z",
  "roles": [
    {
      "id": "7c9e6679-7425-40de-944b-e07fc1f90ae7",
      "name": "Admin",
      "type": "System"
    }
  ]
}
```

### Response 404 Not Found

```json
{
  "title": "User.NotFound",
  "detail": "Người dùng không tồn tại.",
  "status": 404
}
```

### Response 400 Bad Request — ID không hợp lệ

```json
{
  "title": "Validation.InvalidId",
  "detail": "ID phải là GUID hợp lệ.",
  "status": 400
}
```

---

## POST /api/users — Tạo User mới

### Request Body

```json
{
  "firstName": "Nguyen",
  "lastName": "Van A",
  "username": "nguyenvana",
  "password": "P@ssw0rd123",
  "email": "a@example.com",
  "phone": "0901234567",
  "birthday": "1990-05-15"
}
```

| Trường | Kiểu | Bắt buộc | Ràng buộc |
|--------|------|----------|-----------|
| `firstName` | string | Có | Không rỗng, max 100 |
| `lastName` | string | Có | Không rỗng, max 100 |
| `username` | string | Có | Không rỗng, max 50, unique |
| `password` | string | Có | Thoả quy tắc password (min 8 ký tự, có chữ hoa/thường/số) |
| `email` | string? | Không | Định dạng email hợp lệ nếu cung cấp |
| `phone` | string? | Không | Max 20 |
| `birthday` | DateOnly? | Không | Định dạng `yyyy-MM-dd` |

### Response 201 Created

```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "username": "nguyenvana",
  "firstName": "Nguyen",
  "lastName": "Van A"
}
```

**Header**: `Location: /api/users/3fa85f64-5717-4562-b3fc-2c963f66afa6`

### Response 400 Bad Request — Validation

```json
{
  "title": "User.ValidationFailed",
  "detail": "...",
  "status": 400,
  "errors": {
    "username": ["Username không được để trống."]
  }
}
```

### Response 409 Conflict — Username đã tồn tại

```json
{
  "title": "User.UsernameAlreadyTaken",
  "detail": "Username đã được sử dụng.",
  "status": 409
}
```

---

## PUT /api/users/{id} — Cập nhật User

### Request

| Tham số | Vị trí | Kiểu | Bắt buộc |
|---------|--------|------|----------|
| `id` | path | Guid | Có |

### Request Body

```json
{
  "firstName": "Nguyen",
  "lastName": "Van B",
  "email": "b@example.com",
  "phone": "0901234568",
  "birthday": "1990-05-16"
}
```

| Trường | Kiểu | Bắt buộc | Ràng buộc |
|--------|------|----------|-----------|
| `firstName` | string | Có | Không rỗng, max 100 |
| `lastName` | string | Có | Không rỗng, max 100 |
| `email` | string? | Không | Định dạng email hợp lệ nếu cung cấp |
| `phone` | string? | Không | Max 20 |
| `birthday` | DateOnly? | Không | Định dạng `yyyy-MM-dd` |

**Lưu ý**: `username` và `password` không thể đổi qua endpoint này.

### Response 200 OK

```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "firstName": "Nguyen",
  "lastName": "Van B",
  "email": "b@example.com",
  "phone": "0901234568",
  "birthday": "1990-05-16"
}
```

### Response 404 Not Found

```json
{
  "title": "User.NotFound",
  "detail": "Người dùng không tồn tại.",
  "status": 404
}
```

---

## DELETE /api/users/{id} — Xóa User

### Request

| Tham số | Vị trí | Kiểu | Bắt buộc |
|---------|--------|------|----------|
| `id` | path | Guid | Có |

### Response 204 No Content

Xóa thành công. Không có body.

### Response 404 Not Found

```json
{
  "title": "User.NotFound",
  "detail": "Người dùng không tồn tại.",
  "status": 404
}
```

**Lưu ý**: Hard delete — bản ghi bị xóa hoàn toàn. Refresh token (nếu còn hiệu lực) tự vô hiệu vì user không còn trong DB.

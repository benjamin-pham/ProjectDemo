# Contract: Role Endpoints

**Tính năng**: `001-crud-user-role` | **Base URL**: `/api/roles`  
**Xác thực**: Tất cả endpoints yêu cầu `Authorization: Bearer <jwt_token>`

---

## GET /api/roles — Danh sách Role (có phân trang)

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
      "id": "7c9e6679-7425-40de-944b-e07fc1f90ae7",
      "name": "Admin",
      "description": "Toàn quyền quản trị hệ thống",
      "type": "System",
      "permissions": ["users:read", "users:write", "roles:read", "roles:write"],
      "createdAt": "2026-04-01T08:00:00Z"
    }
  ],
  "totalCount": 5,
  "page": 1,
  "pageSize": 20
}
```

### Response 401 Unauthorized

Khi thiếu hoặc token không hợp lệ.

---

## GET /api/roles/{id} — Chi tiết Role

### Request

| Tham số | Vị trí | Kiểu | Bắt buộc |
|---------|--------|------|----------|
| `id` | path | Guid | Có |

### Response 200 OK

```json
{
  "id": "7c9e6679-7425-40de-944b-e07fc1f90ae7",
  "name": "Admin",
  "description": "Toàn quyền quản trị hệ thống",
  "type": "System",
  "permissions": ["users:read", "users:write", "roles:read", "roles:write"],
  "createdAt": "2026-04-01T08:00:00Z",
  "updatedAt": null
}
```

### Response 404 Not Found

```json
{
  "title": "Role.NotFound",
  "detail": "Vai trò không tồn tại.",
  "status": 404
}
```

---

## POST /api/roles — Tạo Role mới

### Request Body

```json
{
  "name": "Editor",
  "description": "Quyền chỉnh sửa nội dung",
  "type": "Dynamic",
  "permissions": ["content:read", "content:write"]
}
```

| Trường | Kiểu | Bắt buộc | Ràng buộc |
|--------|------|----------|-----------|
| `name` | string | Có | Không rỗng, max 100, unique |
| `description` | string | Có | Không rỗng, max 500 |
| `type` | string | Có | Phải là `"System"` hoặc `"Dynamic"` |
| `permissions` | string[] | Có | Mảng chuỗi tự do (có thể rỗng `[]`) |

### Response 201 Created

```json
{
  "id": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
  "name": "Editor",
  "description": "Quyền chỉnh sửa nội dung",
  "type": "Dynamic",
  "permissions": ["content:read", "content:write"]
}
```

**Header**: `Location: /api/roles/a1b2c3d4-e5f6-7890-abcd-ef1234567890`

### Response 409 Conflict — Tên đã tồn tại

```json
{
  "title": "Role.NameAlreadyTaken",
  "detail": "Tên vai trò đã được sử dụng.",
  "status": 409
}
```

### Response 400 Bad Request — Validation

```json
{
  "title": "Role.ValidationFailed",
  "detail": "...",
  "status": 400,
  "errors": {
    "name": ["Tên vai trò không được để trống."],
    "type": ["Loại vai trò không hợp lệ."]
  }
}
```

---

## PUT /api/roles/{id} — Cập nhật Role

### Request

| Tham số | Vị trí | Kiểu | Bắt buộc |
|---------|--------|------|----------|
| `id` | path | Guid | Có |

### Request Body

```json
{
  "name": "Editor",
  "description": "Quyền chỉnh sửa nội dung và media",
  "type": "Dynamic",
  "permissions": ["content:read", "content:write", "media:upload"]
}
```

Cùng ràng buộc với POST.

### Response 200 OK

```json
{
  "id": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
  "name": "Editor",
  "description": "Quyền chỉnh sửa nội dung và media",
  "type": "Dynamic",
  "permissions": ["content:read", "content:write", "media:upload"]
}
```

### Response 404 Not Found

```json
{
  "title": "Role.NotFound",
  "detail": "Vai trò không tồn tại.",
  "status": 404
}
```

### Response 409 Conflict — Tên đã tồn tại (trùng với role khác)

```json
{
  "title": "Role.NameAlreadyTaken",
  "detail": "Tên vai trò đã được sử dụng.",
  "status": 409
}
```

---

## DELETE /api/roles/{id} — Xóa Role

### Request

| Tham số | Vị trí | Kiểu | Bắt buộc |
|---------|--------|------|----------|
| `id` | path | Guid | Có |

### Response 204 No Content

Xóa thành công. Không có body.

### Response 404 Not Found

```json
{
  "title": "Role.NotFound",
  "detail": "Vai trò không tồn tại.",
  "status": 404
}
```

### Response 409 Conflict — Role đang được gán cho User

```json
{
  "title": "Role.HasActiveAssignments",
  "detail": "Không thể xóa vai trò đang được gán cho người dùng.",
  "status": 409
}
```

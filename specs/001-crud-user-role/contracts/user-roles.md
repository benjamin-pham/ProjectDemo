# Contract: User-Role Assignment Endpoints

**Tính năng**: `001-crud-user-role` | **Base URL**: `/api/users/{id}/roles`  
**Xác thực**: Tất cả endpoints yêu cầu `Authorization: Bearer <jwt_token>`

---

## POST /api/users/{id}/roles — Gán Role cho User

Gán một hoặc nhiều Role cho User. Idempotent: nếu Role đã được gán thì bỏ qua, trả về 200 OK.

### Request

| Tham số | Vị trí | Kiểu | Bắt buộc |
|---------|--------|------|----------|
| `id` | path | Guid | Có |

### Request Body

```json
{
  "roleIds": [
    "7c9e6679-7425-40de-944b-e07fc1f90ae7",
    "a1b2c3d4-e5f6-7890-abcd-ef1234567890"
  ]
}
```

| Trường | Kiểu | Bắt buộc | Ràng buộc |
|--------|------|----------|-----------|
| `roleIds` | Guid[] | Có | Không rỗng, mỗi ID phải là Guid hợp lệ |

### Response 200 OK

```json
{
  "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "roles": [
    {
      "id": "7c9e6679-7425-40de-944b-e07fc1f90ae7",
      "name": "Admin",
      "type": "System"
    },
    {
      "id": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
      "name": "Editor",
      "type": "Dynamic"
    }
  ]
}
```

### Response 404 Not Found — User không tồn tại

```json
{
  "title": "User.NotFound",
  "detail": "Người dùng không tồn tại.",
  "status": 404
}
```

### Response 404 Not Found — Một hoặc nhiều Role không tồn tại

```json
{
  "title": "Role.NotFound",
  "detail": "Một hoặc nhiều vai trò không tồn tại.",
  "status": 404
}
```

### Response 400 Bad Request — Validation

```json
{
  "title": "Validation.InvalidRequest",
  "detail": "...",
  "status": 400,
  "errors": {
    "roleIds": ["Danh sách vai trò không được để trống."]
  }
}
```

---

## DELETE /api/users/{id}/roles/{roleId} — Gỡ Role khỏi User

Gỡ một Role cụ thể khỏi User.

### Request

| Tham số | Vị trí | Kiểu | Bắt buộc |
|---------|--------|------|----------|
| `id` | path | Guid | Có |
| `roleId` | path | Guid | Có |

### Response 204 No Content

Gỡ thành công. Không có body.

### Response 404 Not Found — User không tồn tại

```json
{
  "title": "User.NotFound",
  "detail": "Người dùng không tồn tại.",
  "status": 404
}
```

### Response 404 Not Found — Role không được gán cho User này

```json
{
  "title": "Role.NotAssigned",
  "detail": "Vai trò này không được gán cho người dùng.",
  "status": 404
}
```

---

## Ghi chú về Idempotency

- **Gán lại Role đã có**: `POST /api/users/{id}/roles` với `roleIds` chứa Role đã gán → bỏ qua Role đó, gán những Role chưa có, trả về 200 OK với danh sách roles hiện tại. Không lỗi.
- **Gỡ Role chưa được gán**: Trả về 404 `Role.NotAssigned` — client nên xử lý trường hợp này.

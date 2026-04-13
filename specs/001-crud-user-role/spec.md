# Đặc tả tính năng: CRUD API cho User và Role

<!-- Language: Vietnamese — all prose, headings, and descriptions in Vietnamese.
     Code, file paths, identifiers, and code comments remain in English. -->

**Nhánh tính năng**: `001-crud-user-role`
**Ngày tạo**: 2026-04-11
**Trạng thái**: Bản nháp
**Đầu vào**: Mô tả người dùng: "xây dựng api crud @src/MyProject.Domain/Entities/User.cs và @src/MyProject.Domain/Entities/Role.cs"

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Quản lý người dùng (Priority: P1)

Quản trị viên cần xem danh sách toàn bộ người dùng trong hệ thống, xem chi tiết từng người dùng, tạo mới, cập nhật thông tin và xóa người dùng khi không còn cần thiết.

**Why this priority**: Quản lý người dùng là chức năng cốt lõi — không có danh sách và thao tác CRUD trên User, hệ thống không thể vận hành được.

**Independent Test**: Có thể kiểm tra độc lập bằng cách gọi các API endpoints của User (GET list, GET by id, POST, PUT, DELETE) và xác nhận dữ liệu được tạo/truy vấn/cập nhật/xóa đúng trong hệ thống.

**Acceptance Scenarios**:

1. **Given** quản trị viên đã đăng nhập, **When** gọi GET danh sách User, **Then** nhận về danh sách người dùng có phân trang với thông tin cơ bản (id, firstName, lastName, username, email, phone, birthday).
2. **Given** quản trị viên đã đăng nhập, **When** gọi GET theo id của một User tồn tại, **Then** nhận về đầy đủ thông tin của người dùng đó.
3. **Given** quản trị viên đã đăng nhập, **When** gửi POST với thông tin hợp lệ (firstName, lastName, username, password, email, phone, birthday), **Then** người dùng mới được tạo và trả về thông tin User (không bao gồm mật khẩu).
4. **Given** quản trị viên đã đăng nhập, **When** gửi PUT cập nhật thông tin hồ sơ của User tồn tại, **Then** thông tin được cập nhật thành công.
5. **Given** quản trị viên đã đăng nhập, **When** gửi DELETE với id của User tồn tại, **Then** User bị xóa khỏi hệ thống và không thể tìm thấy nữa.
6. **Given** dữ liệu không hợp lệ (username trùng, thiếu trường bắt buộc), **When** gửi POST hoặc PUT, **Then** hệ thống trả về lỗi mô tả rõ nguyên nhân.

---

### User Story 2 - Quản lý vai trò (Priority: P2)

Quản trị viên cần xem danh sách vai trò, xem chi tiết, tạo mới, cập nhật và xóa vai trò để kiểm soát phân quyền trong hệ thống.

**Why this priority**: Quản lý Role là nền tảng của hệ thống phân quyền. Sau khi User CRUD hoạt động, việc quản lý Role cho phép gán quyền và điều khiển truy cập.

**Independent Test**: Có thể kiểm tra độc lập bằng cách gọi các API endpoints của Role (GET list, GET by id, POST, PUT, DELETE) và xác nhận dữ liệu chính xác.

**Acceptance Scenarios**:

1. **Given** quản trị viên đã đăng nhập, **When** gọi GET danh sách Role, **Then** nhận về danh sách vai trò (id, name, description, type, permissions).
2. **Given** quản trị viên đã đăng nhập, **When** gọi GET theo id của một Role tồn tại, **Then** nhận về đầy đủ thông tin của vai trò đó bao gồm danh sách permissions.
3. **Given** quản trị viên đã đăng nhập, **When** gửi POST với thông tin hợp lệ (name, description, type, permissions), **Then** vai trò mới được tạo và trả về thông tin Role.
4. **Given** quản trị viên đã đăng nhập, **When** gửi PUT cập nhật thông tin Role tồn tại, **Then** thông tin được cập nhật thành công.
5. **Given** quản trị viên đã đăng nhập, **When** gửi DELETE với id của Role không được gán cho người dùng nào, **Then** Role bị xóa thành công.
6. **Given** tên Role đã tồn tại, **When** gửi POST tạo Role mới với tên trùng, **Then** hệ thống trả về lỗi và không tạo bản ghi mới.

---

### User Story 3 - Gán và gỡ bỏ vai trò cho người dùng (Priority: P3)

Quản trị viên cần gán một hoặc nhiều vai trò cho người dùng, và gỡ bỏ vai trò khỏi người dùng khi không còn phù hợp.

**Why this priority**: Chức năng gán Role cho User kết nối hai thực thể với nhau. Đây là bước cuối sau khi cả User và Role đã được quản lý riêng lẻ.

**Independent Test**: Có thể kiểm tra bằng cách gán Role cho User và xác nhận User có danh sách Role tương ứng khi truy vấn.

**Acceptance Scenarios**:

1. **Given** quản trị viên đã đăng nhập, User và Role tồn tại, **When** gửi yêu cầu gán Role cho User, **Then** User được cập nhật với vai trò mới.
2. **Given** quản trị viên đã đăng nhập, User đang có một Role, **When** gửi yêu cầu gỡ bỏ Role đó khỏi User, **Then** Role bị loại khỏi danh sách của User.
3. **Given** Role không tồn tại hoặc User không tồn tại, **When** gửi yêu cầu gán Role, **Then** hệ thống trả về lỗi 404 với thông báo rõ ràng.

---

### Edge Cases

- Khi xóa User có refresh token còn hiệu lực: hard delete thẳng — token tự vô hiệu vì user không còn trong DB.
- Điều gì xảy ra khi xóa Role đang được gán cho một hoặc nhiều User?
- Gán cùng Role hai lần cho User: idempotent — bỏ qua và trả về 200 OK nếu Role đã được gán.
- Hệ thống xử lý thế nào khi tìm kiếm User/Role với id không hợp lệ (không phải UUID hợp lệ)?

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: Hệ thống PHẢI cung cấp endpoint lấy danh sách User có hỗ trợ phân trang.
- **FR-002**: Hệ thống PHẢI cung cấp endpoint lấy thông tin chi tiết một User theo id.
- **FR-003**: Hệ thống PHẢI cung cấp endpoint tạo mới User với các trường bắt buộc: firstName, lastName, username, password; và tùy chọn: email, phone, birthday.
- **FR-004**: Hệ thống PHẢI mã hóa mật khẩu và không bao giờ trả về mật khẩu hoặc hash trong response.
- **FR-005**: Hệ thống PHẢI đảm bảo username là duy nhất; trả về lỗi khi trùng lặp.
- **FR-006**: Hệ thống PHẢI cung cấp endpoint cập nhật hồ sơ User (firstName, lastName, email, phone, birthday).
- **FR-007**: Hệ thống PHẢI cung cấp endpoint xóa User theo id.
- **FR-008**: Hệ thống PHẢI cung cấp endpoint lấy danh sách Role có hỗ trợ phân trang.
- **FR-009**: Hệ thống PHẢI cung cấp endpoint lấy thông tin chi tiết một Role theo id, bao gồm danh sách permissions.
- **FR-010**: Hệ thống PHẢI cung cấp endpoint tạo mới Role với các trường: name, description, type, permissions. Handler gọi `Role.Create(name, description, type, permissions)` — validation `name` không được rỗng nằm trong entity.
- **FR-011**: Hệ thống PHẢI đảm bảo tên Role là duy nhất; trả về lỗi khi trùng lặp.
- **FR-012**: Hệ thống PHẢI cung cấp endpoint cập nhật Role (name, description, type, permissions).
- **FR-013**: Hệ thống PHẢI cung cấp endpoint xóa Role theo id.
- **FR-014**: Hệ thống PHẢI cung cấp endpoint gán một hoặc nhiều Role cho User.
- **FR-015**: Hệ thống PHẢI cung cấp endpoint gỡ bỏ Role khỏi User.
- **FR-016**: Tất cả các endpoint PHẢI yêu cầu xác thực; chỉ người dùng có quyền quản trị mới được truy cập.
- **FR-017**: Hệ thống PHẢI trả về thông báo lỗi rõ ràng với mã lỗi phù hợp khi dữ liệu không hợp lệ hoặc tài nguyên không tồn tại.

### Key Entities

- **User**: Đại diện cho tài khoản người dùng trong hệ thống. Các thuộc tính: id, firstName, lastName, username, email (tùy chọn), phone (tùy chọn), birthday (tùy chọn). Navigation: `ICollection<Role> Roles` (EF Core implicit many-to-many).
- **Role**: Đại diện cho vai trò/nhóm quyền trong hệ thống. Các thuộc tính: id, name, description, type (loại vai trò), permissions (danh sách chuỗi tự do). Navigation: `ICollection<User> Users` (EF Core implicit many-to-many).
- Không có `UserRole` entity tường minh — EF Core tự quản lý bảng join ẩn (`user_roles`). `Role.UserRoles` hiện tại cần được cập nhật thành `Role.Users`.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Quản trị viên có thể hoàn thành thao tác tạo, đọc, cập nhật, xóa một User trong vòng dưới 30 giây mỗi thao tác.
- **SC-002**: Quản trị viên có thể hoàn thành thao tác tạo, đọc, cập nhật, xóa một Role trong vòng dưới 30 giây mỗi thao tác.
- **SC-003**: 100% các trường hợp dữ liệu không hợp lệ trả về thông báo lỗi có nội dung mô tả nguyên nhân cụ thể.
- **SC-004**: 100% các thao tác nhạy cảm (tạo/xóa/cập nhật) đều yêu cầu xác thực; yêu cầu chưa xác thực bị từ chối với mã lỗi 401.
- **SC-005**: Danh sách User và Role hỗ trợ phân trang, đảm bảo thời gian phản hồi trang đầu tiên dưới 2 giây với ít nhất 10.000 bản ghi trong hệ thống.

## Clarifications

### Session 2026-04-11

- Q: Quan hệ User ↔ Role dùng navigation trực tiếp hay qua join entity tường minh? → A: Navigation trực tiếp cả hai chiều (`User.Roles` + `Role.Users`), EF Core implicit many-to-many, không cần `UserRole` entity.
- Q: `Role.cs` có cần thêm static factory method `Create(...)` theo pattern `User.cs` không? → A: Có — thêm `Role.Create(name, description, type, permissions)` với validation `ArgumentException.ThrowIfNullOrWhiteSpace` cho `name`.
- Q: Khi xóa User có refresh token còn hiệu lực, hệ thống xử lý thế nào? → A: Xóa thẳng (hard delete) — token tự vô hiệu vì user không còn tồn tại trong DB; không cần revoke trước.
- Q: Khi gán cùng Role hai lần cho User, hệ thống xử lý thế nào? → A: Idempotent — nếu Role đã được gán rồi, bỏ qua và trả về 200 OK.

## Assumptions

- Người dùng thực hiện các thao tác CRUD là quản trị viên (admin) đã đăng nhập vào hệ thống.
- Hệ thống xác thực và phân quyền hiện tại sẽ được tái sử dụng để bảo vệ các endpoint mới; không xây dựng lại cơ chế auth.
- Endpoint xóa User và Role là xóa cứng (hard delete); soft delete không nằm trong phạm vi phiên bản này.
- Khi xóa Role đang được gán cho User, hệ thống sẽ từ chối thao tác và trả về lỗi phù hợp để bảo toàn tính toàn vẹn dữ liệu.
- Danh sách permissions của Role là danh sách chuỗi tự do; không có danh mục permissions cố định được định nghĩa trong phiên bản này.
- Phân trang mặc định trả về 20 bản ghi mỗi trang nếu client không chỉ định.
- API trả về dữ liệu dạng JSON theo chuẩn RESTful đang được áp dụng trong dự án.

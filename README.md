# CrimsonBookStore - MIS 330 Project

A full-stack web application for buying and selling used textbooks, built with .NET 8 Web API backend and HTML/CSS/JavaScript frontend.

## Project Overview

CrimsonBookStore is a point-of-sale system that allows:
- **Customers** to browse books, add to cart, checkout, and submit books for sale
- **Admins** to manage inventory, review sell submissions, and process orders

## Technology Stack

- **Backend**: .NET 8 Web API with Dapper ORM
- **Database**: MySQL (hosted on AWS RDS)
- **Frontend**: HTML5, CSS3, JavaScript (vanilla)
- **Authentication**: Session-based with BCrypt password hashing

## Project Structure

```
330-Team-8/
├── backend/
│   └── CrimsonBookStore.Api/     # .NET 8 Web API
│       ├── Controllers/           # API endpoints
│       ├── Services/              # Business logic
│       ├── Repositories/          # Data access (Dapper)
│       ├── Models/                # Entity models
│       ├── DTOs/                  # Data transfer objects
│       └── Data/                  # Database connection
├── frontend/                      # HTML/CSS/JS frontend
│   ├── index.html
│   ├── books.html
│   ├── cart.html
│   ├── orders.html
│   ├── sell.html
│   ├── admin.html
│   ├── styles.css
│   └── *.js                      # JavaScript modules
└── README.md
```

## Database Connection

The application connects to MySQL database:
- **Host**: ol5tz0yvwp930510.cbetxkdyhwsb.us-east-1.rds.amazonaws.com
- **Database**: ect0p0v3f58sgooq
- **Connection String**: Configured in `appsettings.json`

## Setup Instructions

### Prerequisites
- .NET 8 SDK
- MySQL Workbench (for database management)
- Modern web browser
- Access to MySQL database server

### Database Setup

**The database is already set up on the shared server.** All team members connect to the same database.

**Important Notes:**
- ✅ **No SQL file needed** - Database is already configured on AWS RDS
- ✅ **Same database for everyone** - All team members share the same database
- ✅ **Connection string is in `appsettings.json`** - Already configured, no changes needed
- ✅ **MySQL Workbench is optional** - Only needed if you want to view/edit data directly. The application works without it!
- ✅ **No individual setup required** - Just clone, run `dotnet run`, and it works!

**Demo Login Accounts** (ready to use):
- **Admin**: `admin@test.com` / `password123`
- **Customer**: `user@test.com` / `password123`
- **Admin**: `admin@demo.com` / `admin123`
- **Customer**: `user@demo.com` / `user123`
- **Customer**: `demo@test.com` / `demo123`

**Note**: The database contains sample data including 20 users, 30 books, orders, and submissions.

### Backend Setup

1. Navigate to the backend directory:
```bash
cd backend/CrimsonBookStore.Api
```

2. Restore NuGet packages:
```bash
dotnet restore
```

3. **Verify connection string** in `appsettings.json`:
   - The connection string is already configured in the file
   - **All team members use the same database** (shared on AWS RDS)
   - No changes needed unless the database credentials change
   - The connection string should be:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "mysql://zcac58hx8u3wz18t:wjckln0r9g1inz1z@ol5tz0yvwp930510.cbetxkdyhwsb.us-east-1.rds.amazonaws.com:3306/ect0p0v3f58sgooq"
     }
   }
   ```

4. Build the project:
```bash
dotnet build
```

5. Run the API:
```bash
dotnet run
```

The API will start on:
- **HTTP**: `http://localhost:5000` (use this one)
- **HTTPS**: `https://localhost:5001` (may have certificate issues)
- **Swagger UI**: `http://localhost:5000/swagger` (if available)

**Note**: The frontend is automatically served from the backend at `http://localhost:5000` - no separate frontend server needed!

### Frontend Setup

**The frontend is automatically served by the backend!** No separate setup needed.

1. **Start the backend** (see Backend Setup above)

2. **Open your browser** and navigate to:
   ```
   http://localhost:5000
   ```

3. The frontend will be served automatically. The API base URL is already configured to use the same origin, so no changes needed.

**Alternative**: If you want to run the frontend separately (not recommended):
- The API base URL in `frontend/api.js` uses `window.location.origin`, so it will automatically match your frontend URL

## Testing the Application

### Step 1: Verify Database Connection

1. **Start the backend API** (see Backend Setup)
2. **Test database connection:**
   - Open browser: `http://localhost:5000/api/health/database`
   - Should return: `{"status":"connected","database":"ect0p0v3f58sgooq","userCount":20,"bookCount":30,...}`
   - If you see errors, verify the database connection string is correct

### Step 2: Test Frontend

1. **Open the application:**
   - Navigate to: `http://localhost:5000`
   - You should see the CrimsonBookStore homepage

2. **Test Login:**
   - Click "Login"
   - Use demo account: `admin@test.com` / `password123`
   - Should successfully log in

3. **Test Features:**
   - Browse books
   - Add to cart
   - Checkout (creates order in database)
   - Submit a book for sale
   - View orders

### Step 3: Verify Data in Database

After testing, verify data was saved:
- Use MySQL Workbench to run SQL queries and verify your changes in the database

## API Endpoints

### Authentication
- `POST /api/auth/register` - Register new user
- `POST /api/auth/login` - Login user

### Books
- `GET /api/books` - List all available books
- `GET /api/books/{id}` - Get book details
- `GET /api/books/search?title=&author=&isbn=&major=&course=` - Search books
- `POST /api/books` - Add book (admin)
- `PUT /api/books/{id}` - Update book (admin)
- `DELETE /api/books/{id}` - Delete book (admin)

### Cart
- `GET /api/cart` - Get cart contents
- `POST /api/cart/add` - Add item to cart
- `PUT /api/cart/update` - Update cart item quantity
- `DELETE /api/cart/remove/{bookId}` - Remove item from cart

### Orders
- `POST /api/orders/checkout` - Create purchase order
- `GET /api/orders/customer/{userId}` - Get customer orders
- `GET /api/orders` - Get all orders (admin)
- `PUT /api/orders/{id}/status` - Update order status (admin)
- `PUT /api/orders/{id}/cancel` - Cancel order

### Sell Submissions
- `POST /api/sell-submissions` - Submit book for sale
- `GET /api/sell-submissions/customer/{userId}` - Get customer submissions
- `GET /api/sell-submissions` - Get all submissions (admin)
- `PUT /api/sell-submissions/{id}/approve` - Approve submission (admin)
- `PUT /api/sell-submissions/{id}/reject` - Reject submission (admin)

### Users
- `GET /api/users` - Get all users (admin)
- `GET /api/users/{id}` - Get user by ID

## Sample Data

The database includes:
- **20 Users**: 2 admins, 5 sellers, 10 buyers, 3 demo accounts
- **30 Books**: Various textbooks across different majors/courses
- **21 Sell Submissions**: Mix of Pending, Approved, and Rejected
- **20 Purchase Orders**: Various statuses (New, Processing, Shipped, Completed, Cancelled)
- **55 Order Line Items**: Matching purchase orders

**Demo Login Accounts** (ready to use):
- `admin@test.com` / `password123` (Admin)
- `user@test.com` / `password123` (Customer)
- `admin@demo.com` / `admin123` (Admin)
- `user@demo.com` / `user123` (Customer)
- `demo@test.com` / `demo123` (Customer)

## Business Logic Implementation

### Inventory Management
- ✅ Unique ISBN constraint
- ✅ StockQuantity increments on sell submission approval
- ✅ StockQuantity decrements atomically on checkout
- ✅ Only "Available" books with StockQuantity > 0 shown to customers
- ✅ SellingPrice must be greater than AcquisitionCost (enforced in service layer)

### Customer Selling
- ✅ Submission status: "Pending" → Admin approves/rejects
- ✅ Approve → Creates/updates Book record, increments StockQuantity
- ✅ Reject → No inventory action

### Customer Buying
- ✅ Cart validation: Only add if StockQuantity >= 1
- ✅ Cart quantity limit: Cannot exceed available StockQuantity
- ✅ Atomic inventory update on checkout
- ✅ Order status flow: New → Processing → Shipped → Completed (or Cancelled)
- ✅ Cancellation restores stock quantities

## Features

### Customer Features
- ✅ Register/Login
- ✅ Browse and search books
- ✅ View book details
- ✅ Add to cart, manage cart
- ✅ Checkout and place orders
- ✅ View purchase history
- ✅ Submit books for sale

### Admin Features
- ✅ View all users
- ✅ Manage book inventory (Add/Edit/Delete)
- ✅ Review sell submissions (Approve/Reject)
- ✅ View all purchase orders
- ✅ Update order status

## Requirements Compliance

See `REQUIREMENTS_VERIFICATION.md` for detailed checklist of all MIS 330 project requirements.

**All functional requirements are implemented:**
- ✅ User authentication (Customer/Admin)
- ✅ Search & browsing
- ✅ Selling transactions
- ✅ Buying transactions with cart/checkout
- ✅ Administrative management
- ✅ All business logic rules
- ✅ Complete database schema
- ✅ Sample data (5 sellers, 10 buyers, 30 books)

## Troubleshooting

### Backend won't start
- Check if port 5000 is available
- Verify connection string in `appsettings.json`
- Ensure database is accessible

### Frontend can't connect to API
- Verify backend is running on `http://localhost:5000`
- Check browser console for CORS errors
- Update `API_BASE_URL` in `frontend/api.js` if needed

### Database connection errors
- Verify MySQL server is accessible
- Check connection string format in `appsettings.json`
- Ensure database `ect0p0v3f58sgooq` exists
- Test connection: `http://localhost:5000/api/health/database`

## Next Steps (Phase 4)

The management queries (Phase 4) should be implemented as separate SQL scripts. These are reporting queries for:
- Inventory and stock management
- Sales and purchase order analysis
- Book acquisition analysis
- User and administration audit

## Quick Start Checklist

**For new team members, follow these steps in order:**

1. ✅ **Start backend**: `cd backend/CrimsonBookStore.Api && dotnet run`
2. ✅ **Open browser**: `http://localhost:5000`
3. ✅ **Test login**: Use `admin@test.com` / `password123`
4. ✅ **Verify database**: Check `http://localhost:5000/api/health/database`

## Team Members

[Add your team members here]

## License

This project is for educational purposes (MIS 330 course).

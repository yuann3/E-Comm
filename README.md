## System Architecture

### Technology Stack
- **Framework**: ASP.NET Core 9.0 MVC
- **Database**: MSSQL Server 2022
- **ORM**: Entity Framework Core 9.0
- **Authentication**: ASP.NET Core Identity with Cookie Authentication
- **Frontend**: Bootstrap 5, FontAwesome 6.4.0, jQuery
  
### Project Structure
```
E-Comm/
├── Controllers/           # MVC Controllers
│   ├── HomeController.cs     # Public pages (Index, Catalog, About)
│   ├── AuthController.cs     # Authentication (Login, Logout)
│   ├── AdminController.cs    # Administrator functions
│   ├── EmployeeController.cs # Employee functions
│   └── CustomerController.cs # Customer functions
├── Models/               # Data models and business logic
│   ├── EntertainmentGuildContext.cs # EF DbContext
│   ├── AuthService.cs               # Authentication service
│   ├── DataSeeder.cs                # Database seeding
│   ├── Product.cs                   # Product entity
│   ├── User.cs                      # User entity
│   ├── Customer.cs                  # Customer entity
│   ├── Order.cs                     # Order entity
│   ├── Genre.cs                     # Genre entity
│   ├── Source.cs                    # Supplier entity
│   ├── Stocktake.cs                 # Inventory entity
│   └── ProductsInOrder.cs           # Order-Product junction
├── Views/                # Razor view templates
│   ├── Shared/              # Layout and shared components
│   ├── Home/                # Public pages
│   ├── Auth/                # Authentication pages
│   ├── Admin/               # Administrator interfaces
│   ├── Employee/            # Employee interfaces
│   └── Customer/            # Customer interfaces
├── wwwroot/              # Static files (CSS, JS, images)
├── Migrations/           # EF Core database migrations
├── Properties/           # Application configuration
└── Program.cs            # Application entry point
```

## Database Design

### Entity Relationship Overview
The database follows the provided SQL schema with the following core entities:

**Primary Entities:**
- `Genre` - Product categories (Books, Movies, Games, etc.)
- `Product` - Entertainment items with metadata
- `Source` - Suppliers/distributors
- `Stocktake` - Inventory management (links Products to Sources)
- `User` - System users (Admin/Employee authentication)
- `Customer` - Customer information (TO table)
- `Order` - Customer orders
- `ProductsInOrder` - Order line items

**Key Relationships:**
- Product belongs to Genre (Many-to-One)
- Stocktake links Product to Source with quantity/price (Many-to-Many)
- Order belongs to Customer (Many-to-One)
- ProductsInOrder links Order to Stocktake (Many-to-Many)

### Database Configuration
The application uses SQLite, Connection string is defined in `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=StoreDB.sqlite"
  }
}
```

## Authentication and Authorization

### User Roles
1. **Customer** - Can browse products, create accounts, place orders, manage profile
2. **Employee** - Can view products, customers, and orders (read-only access)
3. **Administrator** - Full system access including user and product management

### Authentication Implementation
- Cookie-based authentication with 24-hour expiration
- Role-based authorization using ASP.NET Core policies
- Test credentials for development:
  - Customer: `customer@example.com` / `Password1`
  - Employee: `employee@example.com` / `Passw0rd`
  - Administrator: `administrator@example.com` / `Pa$$w0rd`

### Authorization Policies
```csharp
services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("EmployeeOnly", policy => policy.RequireRole("Employee"));
    options.AddPolicy("CustomerOnly", policy => policy.RequireRole("Customer"));
});
```

## Business Requirements Implementation

### Core Scenarios
1. **Customer Product Search** - Implemented in `HomeController.Catalog()` and `CustomerController.Browse()`
2. **Item Management** - Implemented in `AdminController` with CRUD operations
3. **User Account Creation** - Implemented in `CustomerController.CreateAccount()`
4. **Account Management** - Implemented in `CustomerController` with address validation

### Access Control Matrix
| Feature | Customer | Employee | Admin |
|---------|----------|----------|-------|
| Browse Products | Yes | Yes | Yes |
| Purchase Items | Yes | No | No |
| View Customer Data | Own Only | All | All |
| Manage Products | No | No | Yes |
| Manage Users | No | No | Yes |
| View Orders | Own Only | All | All |

## Development Setup

### Prerequisites

### Installation Steps
1. Clone the repository
2. Navigate to the project directory: `cd E-Comm`
3. Restore NuGet packages: `dotnet restore`
4. Apply database migrations: `dotnet ef database update`
5. Run the application: `dotnet run`
6. Access the application at `http://localhost:5000`

### Database Seeding
The application automatically seeds sample data on startup through `DataSeeder.cs`. This includes:
- 10 genres (Books, Movies, Games, Fiction, Sci-Fi, etc.)
- 12 sample products (4 books, 4 movies, 4 games)
- 5 suppliers/sources
- Inventory records with realistic pricing
- Sample customer account

## API Endpoints and Routing

### Public Routes
- `GET /` - Homepage with featured products
- `GET /Home/Catalog` - Product catalog with search/filter
- `GET /Home/ProductDetails/{id}` - Product detail page
- `GET /Home/About` - About page
- `GET /Auth/Login` - Login page
- `POST /Auth/Login` - Authentication
- `POST /Auth/Logout` - Logout

### Customer Routes (Requires Customer Role)
- `GET /Customer` - Customer dashboard
- `GET /Customer/Browse` - Product browsing
- `GET /Customer/MyAccount` - Account management
- `POST /Customer/CreateAccount` - Account creation
- `GET /Customer/OrderHistory` - Order history
- `POST /Customer/AddToCart` - Add items to cart

### Employee Routes (Requires Employee Role)
- `GET /Employee` - Employee dashboard
- `GET /Employee/ViewItems` - Product inventory
- `GET /Employee/ViewAccounts` - Customer accounts
- `GET /Employee/ViewOrders` - All orders
- `GET /Employee/SearchItems` - Product search

### Admin Routes (Requires Admin Role)
- `GET /Admin` - Admin dashboard
- `GET /Admin/ManageItems` - Product management
- `POST /Admin/CreateItem` - Create product
- `GET /Admin/ManageUsers` - User management
- `GET /Admin/Reports` - System reports

## Data Models

### Core Entity Definitions

**Product Model:**
```csharp
public class Product
{
    public int ID { get; set; }
    public string Name { get; set; }
    public string? Author { get; set; }
    public string? Description { get; set; }
    public int? GenreID { get; set; }
    public DateTime? Published { get; set; }
    public string? LastUpdatedBy { get; set; }
    public DateTime? LastUpdated { get; set; }
    
    public virtual Genre? Genre { get; set; }
    public virtual ICollection<Stocktake> Stocktakes { get; set; }
}
```

**Customer Model:**
```csharp
public class Customer
{
    public int CustomerID { get; set; }
    public string Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string? StreetAddress { get; set; }
    public int? PostCode { get; set; }
    public string? State { get; set; }
    // Payment and address fields...
    
    public virtual ICollection<Order> Orders { get; set; }
}
```

## Frontend Implementation

### UI Framework
- Bootstrap 5 
- FontAwesome 6.4.0 for icons
- Custom CSS for 3B1G branding
- jQuery for client-side interactions

### Key UI Components
- Responsive navigation with role-based menus
- Product cards with hover effects
- Search and filter interfaces
- Modal dialogs for forms
- Toast notifications for user feedback
- Pagination for large datasets

### Responsive Design
The application is mobile-first with breakpoints:
- xs: <576px (mobile)
- sm: ≥576px (large mobile)
- md: ≥768px (tablet)
- lg: ≥992px (desktop)
- xl: ≥1200px (large desktop)

## Development Guidelines

### Code Organization
- **Controllers**: Handle HTTP requests, delegate business logic to services
- **Models**: Define entities and business logic
- **Views**: Razor templates with minimal logic
- **Services**: Business logic and data operations

### Naming Conventions
- Classes: PascalCase (`ProductController`)
- Methods: PascalCase (`GetProductById`)
- Properties: PascalCase (`ProductName`)
- Variables: camelCase (`productId`)
- Database tables: Match entity names
- View files: Match action names

### Error Handling
- Use try-catch blocks for database operations
- Return appropriate HTTP status codes
- Display user-friendly error messages
- Log errors for debugging

## Testing Strategy

### Unit Testing Areas
- Model validation
- Controller actions
- Authentication logic
- Business rule enforcement

### Integration Testing
- Database operations
- Authentication flow
- API endpoints
- View rendering

## Contributing Guidelines

### Adding New Features
1. Create feature branch from main
2. Implement following MVC pattern
3. Add appropriate authorization attributes
4. Create corresponding views
5. Update database if schema changes required
6. Test with all user roles
7. Submit pull request

### Database Changes
1. Create new migration: `dotnet ef migrations add DescriptiveName`
2. Review generated migration code
3. Test migration: `dotnet ef database update`
4. Update seeding data if necessary

### Adding New Controllers
1. Inherit from `Controller`
2. Add appropriate authorization attributes
3. Follow RESTful conventions
4. Implement error handling
5. Create corresponding views
6. Update navigation menus

### Common Development Tasks

**Adding New Product Properties:**
1. Update `Product` model
2. Create and apply migration
3. Update views and forms
4. Modify seeding data
5. Update search functionality

**Adding New User Role:**
1. Update authentication configuration
2. Create new controller with role authorization
3. Add role-specific navigation
4. Create role-specific views
5. Update test credentials

**Modifying UI Components:**
1. Update Razor views
2. Modify CSS in layout
3. Test responsive behavior
4. Ensure accessibility compliance
5. Test with different user roles

## Troubleshooting

### Common Issues
- **Database Connection**: Verify connection string in appsettings.json
- **Migration Errors**: Ensure database is accessible and previous migrations applied
- **Authentication Issues**: Check role configuration and cookie settings
- **Build Errors**: Restore NuGet packages and check .NET version

### Performance Considerations
- Use Include() for loading related data
- Implement pagination for large datasets
- Add indexes for frequently queried columns
- Use async/await for database operations
- Optimize image sizes and static assets

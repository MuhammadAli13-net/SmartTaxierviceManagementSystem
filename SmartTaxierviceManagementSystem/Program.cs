using System;

namespace SmartTaxierviceManagementSystem
{
    // ENUMLAR
    enum DriverStatus { Free, Busy, Resting, Offline }
    enum OrderStatus { Waiting, Accepted, OnTheWay, Completed, Canceled }
    enum CarType { Econom, Comfort, Business, Premium }
    enum Rank { None, Silver, Gold, Platinum }

    // STATIC COMPANY INFO & COUNTERS
    static class Company
    {
        public static string CompanyName = "SMART TAXI UZBEKISTAN";
        public static int DriverCounter = 0;
        public static int CustomerCounter = 0;
        public static int OrderCounter = 0;
        public static decimal CompanyIncome = 0m;
    }

    // STRUCT Location
    struct Location
    {
        public string Region;
        public string District;
        public string Street;
        public string HouseNumber;

        public string ShowAddress()
        {
            return $"{Region}, {District}, {Street}, {HouseNumber}";
        }
    }

    // DRIVER CLASS
    class Driver
    {
        // Properties
        public int Id { get; set; }
        public string FullName { get; set; }
        public string PhoneNumber { get; set; }
        public string CarModel { get; set; }
        public DriverStatus Status { get; set; } = DriverStatus.Free;
        public CarType CarType { get; set; }
        public Rank DriverRank
        {
            get
            {
                if (_completedOrders >= 1000) return Rank.Platinum;
                if (_completedOrders >= 500) return Rank.Gold;
                if (_completedOrders >= 100) return Rank.Silver;
                return Rank.None;
            }
        }

        // Private fields
        private decimal _salary = 0m;
        private int _completedOrders = 0;
        private double _rating = 5.0;

        // Parameterless constructor for object initializers
        public Driver()
        {
            Company.DriverCounter++;
        }

        // Parameterized constructor (uses this)
        public Driver(int id, string fullName, string phoneNumber, string carModel, CarType carType)
            : this()
        {
            this.Id = id;
            this.FullName = fullName;
            this.PhoneNumber = phoneNumber;
            this.CarModel = carModel;
            this.CarType = carType;
        }

        // Methods
        public void AcceptOrder(Order order)
        {
            if (order == null) return;
            if (this.Status != DriverStatus.Free)
            {
                Console.WriteLine($"Driver {FullName} is not free to accept the order.");
                return;
            }

            this.Status = DriverStatus.Busy;
            order.AssignDriver(this);
            Console.WriteLine($"Order {order.OrderId} assigned to driver {FullName}.");
        }

        public void FinishOrder(Order order)
        {
            if (order == null) return;
            if (order.Status != OrderStatus.OnTheWay && order.Status != OrderStatus.Accepted)
            {
                Console.WriteLine("Order is not in progress.");
                return;
            }

            order.CompleteTrip(this);
        }

        public void TakeBreak()
        {
            if (this.Status == DriverStatus.Busy)
            {
                Console.WriteLine("Driver is busy and cannot take a break now.");
                return;
            }
            this.Status = DriverStatus.Resting;
            Console.WriteLine($"{FullName} is now resting.");
        }

        public void BackToWork()
        {
            if (this.Status == DriverStatus.Busy) return;
            this.Status = DriverStatus.Free;
            Console.WriteLine($"{FullName} is back to work.");
        }

        public void IncreaseSalary(decimal amount)
        {
            if (amount <= 0) return;
            _salary += amount;
            Console.WriteLine($"{FullName}'s salary increased by {amount:C}. Total salary: {_salary:C}");
        }

        internal void NotifyOrderCompleted(decimal price)
        {
            _completedOrders++;
            _salary += price * 0.4m; // driver earnings example
            _rating = Math.Min(5.0, _rating + 0.01); // small rating increase
        }

        public void ShowInfo()
        {
            Console.WriteLine("----------- DRIVER INFO -----------");
            Console.WriteLine($"Id: {Id}");
            Console.WriteLine($"Name: {FullName}");
            Console.WriteLine($"Phone: {PhoneNumber}");
            Console.WriteLine($"CarModel: {CarModel} ({CarType})");
            Console.WriteLine($"Status: {Status}");
            Console.WriteLine($"CompletedOrders: {_completedOrders}");
            Console.WriteLine($"Salary: {_salary:C}");
            Console.WriteLine($"Rating: {_rating:F2}");
            Console.WriteLine($"Rank: {DriverRank}");
            Console.WriteLine("-----------------------------------");
        }

        ~Driver()
        {
            // Destructor (finalizer) - may run non-deterministically
            Console.WriteLine("Driver object removed from memory...");
        }

        // Expose completed orders for stats (read-only)
        public int GetCompletedOrders() => _completedOrders;
    }

    // CUSTOMER CLASS
    class Customer
    {
        public int CustomerId { get; set; }
        public string FullName { get; set; }
        public string PhoneNumber { get; set; }

        public Customer()
        {
            Company.CustomerCounter++;
        }

        public void ShowCustomerInfo()
        {
            Console.WriteLine("--------- CUSTOMER INFO ----------");
            Console.WriteLine($"Id: {CustomerId}");
            Console.WriteLine($"Name: {FullName}");
            Console.WriteLine($"Phone: {PhoneNumber}");
            Console.WriteLine("----------------------------------");
        }
    }

    // ORDER CLASS
    class Order
    {
        public int OrderId { get; set; }
        public string CustomerName { get; set; }
        public string DriverName { get; set; } = "Unassigned";
        public Location StartLocation { get; set; }
        public Location EndLocation { get; set; }
        public OrderStatus Status { get; set; } = OrderStatus.Waiting;
        public decimal Price { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public Order()
        {
            Company.OrderCounter++;
        }

        public void AssignDriver(Driver driver)
        {
            if (driver == null)
            {
                Console.WriteLine("Driver is null.");
                return;
            }

            DriverName = driver.FullName;
            Status = OrderStatus.Accepted;
            Console.WriteLine($"Order {OrderId} accepted by {DriverName}.");
        }

        public void StartTrip()
        {
            if (Status != OrderStatus.Accepted) return;
            Status = OrderStatus.OnTheWay;
            Console.WriteLine($"Order {OrderId} is now OnTheWay.");
        }

        public void CompleteTrip(Driver driver)
        {
            if (Status == OrderStatus.Completed || Status == OrderStatus.Canceled) return;

            Status = OrderStatus.Completed;
            Company.CompanyIncome += Price;

            if (driver != null)
            {
                // Inform driver about completion so driver updates private fields
                driver.NotifyOrderCompleted(Price);
                driver.BackToWork();
            }

            Console.WriteLine($"Order {OrderId} completed. Price {Price:C} added to company income.");
        }

        public void CancelTrip()
        {
            if (Status == OrderStatus.Completed) return;
            Status = OrderStatus.Canceled;
            Console.WriteLine($"Order {OrderId} has been canceled.");
        }

        public void ShowOrderInfo()
        {
            Console.WriteLine("----------- ORDER INFO -----------");
            Console.WriteLine($"OrderId: {OrderId}");
            Console.WriteLine($"Customer: {CustomerName}");
            Console.WriteLine($"Driver: {DriverName}");
            Console.WriteLine($"From: {StartLocation.ShowAddress()}");
            Console.WriteLine($"To: {EndLocation.ShowAddress()}");
            Console.WriteLine($"Status: {Status}");
            Console.WriteLine($"Price: {Price:C}");
            Console.WriteLine($"Created: {CreatedDate}");
            Console.WriteLine("----------------------------------");
        }
    }

    class Program
    {
        // ARRAYS (no Lists allowed)
        static Driver[] drivers = new Driver[30];
        static Customer[] customers = new Customer[100];
        static Order[] orders = new Order[200];

        static void Main(string[] args)
        {
            // Initialize sample data with object initializers (at least 5 each)
            SeedDrivers();
            SeedCustomers();
            SeedOrdersSample();

            while (true)
            {
                ShowMainMenu();
                var choice = Console.ReadLine();
                Console.WriteLine();
                if (!int.TryParse(choice, out int option)) option = -1;

                switch (option)
                {
                    case 1: AddDriver(); break;
                    case 2: ListDrivers(); break;
                    case 3: AddCustomer(); break;
                    case 4: ListCustomers(); break;
                    case 5: CreateOrder(); break;
                    case 6: ListOrders(); break;
                    case 7: StartOrder(); break;
                    case 8: FinishOrder(); break;
                    case 9: CancelOrder(); break;
                    case 10: ShowStatistics(); break;
                    case 11: ExitApp(); return;
                    default: Console.WriteLine("Invalid option. Try again."); break;
                }

                Console.WriteLine("\nPress Enter to continue...");
                Console.ReadLine();
                Console.Clear();
            }
        }

        static void ShowMainMenu()
        {
            Console.WriteLine("╔════════════════════════════════════╗");
            Console.WriteLine("║     SMART TAXI UZBEKISTAN          ║");
            Console.WriteLine("╠════════════════════════════════════╣");
            Console.WriteLine("║ 1. Haydovchi qo'shish              ║");
            Console.WriteLine("║ 2. Haydovchilar ro'yxati           ║");
            Console.WriteLine("║ 3. Mijoz qo'shish                  ║");
            Console.WriteLine("║ 4. Mijozlar ro'yxati               ║");
            Console.WriteLine("║ 5. Buyurtma yaratish               ║");
            Console.WriteLine("║ 6. Buyurtmalar ro'yxati            ║");
            Console.WriteLine("║ 7. Buyurtmani boshlash             ║");
            Console.WriteLine("║ 8. Buyurtmani yakunlash            ║");
            Console.WriteLine("║ 9. Buyurtmani bekor qilish         ║");
            Console.WriteLine("║10. Statistika                      ║");
            Console.WriteLine("║11. Chiqish                         ║");
            Console.WriteLine("╚════════════════════════════════════╝");
            Console.Write("Select option: ");
        }

        static void SeedDrivers()
        {
            drivers[0] = new Driver() { Id = 1, FullName = "Ali Valiyev", PhoneNumber = "+998901112233", CarModel = "Nexia 3", CarType = CarType.Econom };
            drivers[1] = new Driver() { Id = 2, FullName = "Bekzod Karimov", PhoneNumber = "+998909998877", CarModel = "Cobalt", CarType = CarType.Comfort };
            drivers[2] = new Driver() { Id = 3, FullName = "Dilkash Ergasheva", PhoneNumber = "+998977554433", CarModel = "Toyota Camry", CarType = CarType.Business };
            drivers[3] = new Driver() { Id = 4, FullName = "Elmurod Sobirov", PhoneNumber = "+998901223344", CarModel = "Honda Civic", CarType = CarType.Comfort };
            drivers[4] = new Driver() { Id = 5, FullName = "Farida Isroilova", PhoneNumber = "+998903334455", CarModel = "BMW 5", CarType = CarType.Premium };

            // Update DriverCounter correct value (constructor increments; we used 5)
            // Note: Company.DriverCounter already incremented in constructors above.
        }

        static void SeedCustomers()
        {
            customers[0] = new Customer() { CustomerId = 1, FullName = "Olimjon Rasulov", PhoneNumber = "+998901111000" };
            customers[1] = new Customer() { CustomerId = 2, FullName = "Nilufar Saidova", PhoneNumber = "+998901112222" };
            customers[2] = new Customer() { CustomerId = 3, FullName = "Jaloliddin Azimov", PhoneNumber = "+998901113333" };
            customers[3] = new Customer() { CustomerId = 4, FullName = "Sabina Kadirova", PhoneNumber = "+998901114444" };
            customers[4] = new Customer() { CustomerId = 5, FullName = "Murodjon Khudoyberdiyev", PhoneNumber = "+998901115555" };
        }

        static void SeedOrdersSample()
        {
            // A couple of sample orders (not required but helpful)
            orders[0] = new Order()
            {
                OrderId = 1,
                CustomerName = customers[0]?.FullName ?? "Unknown",
                StartLocation = new Location { Region = "Tashkent", District = "Yunusobod", Street = "Amir Temur", HouseNumber = "12" },
                EndLocation = new Location { Region = "Tashkent", District = "Mirabad", Street = "Bunyodkor", HouseNumber = "45" },
                Price = 12.50m
            };
            orders[1] = new Order()
            {
                OrderId = 2,
                CustomerName = customers[1]?.FullName ?? "Unknown",
                StartLocation = new Location { Region = "Tashkent", District = "Chilonzor", Street = "Qahramon", HouseNumber = "7" },
                EndLocation = new Location { Region = "Tashkent", District = "Yashnobod", Street = "Bog'ishamol", HouseNumber = "3" },
                Price = 8.75m
            };
        }

        static int FindNextDriverIndex()
        {
            for (int i = 0; i < drivers.Length; i++) if (drivers[i] == null) return i;
            return -1;
        }

        static int FindNextCustomerIndex()
        {
            for (int i = 0; i < customers.Length; i++) if (customers[i] == null) return i;
            return -1;
        }

        static int FindNextOrderIndex()
        {
            for (int i = 0; i < orders.Length; i++) if (orders[i] == null) return i;
            return -1;
        }

        static void AddDriver()
        {
            int idx = FindNextDriverIndex();
            if (idx == -1) { Console.WriteLine("Driver array full."); return; }

            var driver = new Driver();
            Console.Write("Id: "); driver.Id = ReadInt();
            Console.Write("FullName: "); driver.FullName = Console.ReadLine();
            Console.Write("PhoneNumber: "); driver.PhoneNumber = Console.ReadLine();
            Console.Write("CarModel: "); driver.CarModel = Console.ReadLine();
            Console.WriteLine("CarType (0=Econom,1=Comfort,2=Business,3=Premium): ");
            if (int.TryParse(Console.ReadLine(), out int ct) && Enum.IsDefined(typeof(CarType), ct))
                driver.CarType = (CarType)ct;
            drivers[idx] = driver;
            Console.WriteLine("Driver added successfully.");
        }

        static void ListDrivers()
        {
            Console.WriteLine("----- DRIVERS LIST -----");
            for (int i = 0; i < drivers.Length; i++)
            {
                if (drivers[i] != null)
                {
                    Console.WriteLine($"{drivers[i].Id}. {drivers[i].FullName} - {drivers[i].PhoneNumber} - {drivers[i].Status} - {drivers[i].CarType}");
                }
            }
        }

        static void AddCustomer()
        {
            int idx = FindNextCustomerIndex();
            if (idx == -1) { Console.WriteLine("Customer array full."); return; }

            var customer = new Customer();
            Console.Write("CustomerId: "); customer.CustomerId = ReadInt();
            Console.Write("FullName: "); customer.FullName = Console.ReadLine();
            Console.Write("PhoneNumber: "); customer.PhoneNumber = Console.ReadLine();
            customers[idx] = customer;
            Console.WriteLine("Customer added successfully.");
        }

        static void ListCustomers()
        {
            Console.WriteLine("----- CUSTOMERS LIST -----");
            for (int i = 0; i < customers.Length; i++)
            {
                if (customers[i] != null)
                {
                    Console.WriteLine($"{customers[i].CustomerId}. {customers[i].FullName} - {customers[i].PhoneNumber}");
                }
            }
        }

        static void CreateOrder()
        {
            int idx = FindNextOrderIndex();
            if (idx == -1) { Console.WriteLine("Orders array full."); return; }

            var order = new Order();
            Console.Write("OrderId: "); order.OrderId = ReadInt();
            Console.Write("CustomerId (enter existing id): "); int custId = ReadInt();
            var cust = FindCustomerById(custId);
            if (cust == null)
            {
                Console.WriteLine("Customer not found. Aborting.");
                return;
            }
            order.CustomerName = cust.FullName;

            Console.WriteLine("Start Location:");
            order.StartLocation = ReadLocation();
            Console.WriteLine("End Location:");
            order.EndLocation = ReadLocation();

            Console.Write("Price: "); order.Price = ReadDecimal();

            orders[idx] = order;
            Console.WriteLine($"Order {order.OrderId} created and waiting for assignment.");
        }

        static void ListOrders()
        {
            Console.WriteLine("----- ORDERS LIST -----");
            for (int i = 0; i < orders.Length; i++)
            {
                if (orders[i] != null)
                {
                    Console.WriteLine($"{orders[i].OrderId}. {orders[i].CustomerName} -> {orders[i].DriverName} [{orders[i].Status}] Price: {orders[i].Price:C}");
                }
            }
        }

        static void StartOrder()
        {
            Console.Write("Enter OrderId to start: ");
            int id = ReadInt();
            var order = FindOrderById(id);
            if (order == null) { Console.WriteLine("Order not found."); return; }

            if (order.Status == OrderStatus.Waiting)
            {
                // assign first free driver automatically
                var drv = FindFirstFreeDriver();
                if (drv == null) { Console.WriteLine("No free driver available."); return; }
                drv.AcceptOrder(order);
            }

            if (order.Status == OrderStatus.Accepted)
            {
                order.StartTrip();
            }
        }

        static void FinishOrder()
        {
            Console.Write("Enter OrderId to finish: ");
            int id = ReadInt();
            var order = FindOrderById(id);
            if (order == null) { Console.WriteLine("Order not found."); return; }

            if (order.Status != OrderStatus.OnTheWay && order.Status != OrderStatus.Accepted)
            {
                Console.WriteLine("Order is not in progress.");
                return;
            }

            var driver = FindDriverByName(order.DriverName);
            order.CompleteTrip(driver);
        }

        static void CancelOrder()
        {
            Console.Write("Enter OrderId to cancel: ");
            int id = ReadInt();
            var order = FindOrderById(id);
            if (order == null) { Console.WriteLine("Order not found."); return; }

            order.CancelTrip();
        }

        static void ShowStatistics()
        {
            Console.WriteLine("═══════════════════════════════");
            Console.WriteLine("      SYSTEM ANALYTICS");
            Console.WriteLine("═══════════════════════════════");
            Console.WriteLine($"Company: {Company.CompanyName}\n");
            Console.WriteLine($"Jami haydovchilar soni: {CountExistingDrivers()}");
            Console.WriteLine($"Jami mijozlar soni: {CountExistingCustomers()}");
            Console.WriteLine($"Jami buyurtmalar soni: {CountExistingOrders()}\n");

            Console.WriteLine($"Bo'sh haydovchilar: {CountDriversByStatus(DriverStatus.Free)}");
            Console.WriteLine($"Band haydovchilar: {CountDriversByStatus(DriverStatus.Busy)}");
            Console.WriteLine($"Tanaffusdagi haydovchilar: {CountDriversByStatus(DriverStatus.Resting)}\n");

            Console.WriteLine($"Bajarilgan buyurtmalar: {CountOrdersByStatus(OrderStatus.Completed)}");
            Console.WriteLine($"Bekor qilingan buyurtmalar: {CountOrdersByStatus(OrderStatus.Canceled)}");
            Console.WriteLine($"Jarayondagi buyurtmalar: {CountOrdersByStatus(OrderStatus.OnTheWay)}\n");

            Console.WriteLine($"Kompaniya daromadi: {Company.CompanyIncome:C}");

            var top = FindMostActiveDriver();
            Console.WriteLine($"Eng faol haydovchi: {(top != null ? top.FullName + $" ({top.GetCompletedOrders()} orders)" : "N/A")}");

            Console.WriteLine("═══════════════════════════════");
        }

        static void ExitApp()
        {
            Console.WriteLine("Exiting application. Goodbye!");
        }

        // Helper methods
        static int ReadInt()
        {
            while (true)
            {
                var s = Console.ReadLine();
                if (int.TryParse(s, out int v)) return v;
                Console.Write("Invalid input. Enter integer: ");
            }
        }

        static decimal ReadDecimal()
        {
            while (true)
            {
                var s = Console.ReadLine();
                if (decimal.TryParse(s, out decimal v)) return v;
                Console.Write("Invalid input. Enter decimal number: ");
            }
        }

        static Location ReadLocation()
        {
            var loc = new Location();
            Console.Write("Region: "); loc.Region = Console.ReadLine();
            Console.Write("District: "); loc.District = Console.ReadLine();
            Console.Write("Street: "); loc.Street = Console.ReadLine();
            Console.Write("HouseNumber: "); loc.HouseNumber = Console.ReadLine();
            return loc;
        }

        static Customer FindCustomerById(int id)
        {
            for (int i = 0; i < customers.Length; i++) if (customers[i] != null && customers[i].CustomerId == id) return customers[i];
            return null;
        }

        static Order FindOrderById(int id)
        {
            for (int i = 0; i < orders.Length; i++) if (orders[i] != null && orders[i].OrderId == id) return orders[i];
            return null;
        }

        static Driver FindDriverByName(string name)
        {
            if (string.IsNullOrEmpty(name)) return null;
            for (int i = 0; i < drivers.Length; i++) if (drivers[i] != null && drivers[i].FullName == name) return drivers[i];
            return null;
        }

        static Driver FindFirstFreeDriver()
        {
            for (int i = 0; i < drivers.Length; i++) if (drivers[i] != null && drivers[i].Status == DriverStatus.Free) return drivers[i];
            return null;
        }

        static int CountExistingDrivers()
        {
            int c = 0;
            foreach (var d in drivers) if (d != null) c++;
            return c;
        }

        static int CountExistingCustomers()
        {
            int c = 0;
            foreach (var d in customers) if (d != null) c++;
            return c;
        }

        static int CountExistingOrders()
        {
            int c = 0;
            foreach (var d in orders) if (d != null) c++;
            return c;
        }

        static int CountDriversByStatus(DriverStatus status)
        {
            int c = 0;
            foreach (var d in drivers) if (d != null && d.Status == status) c++;
            return c;
        }

        static int CountOrdersByStatus(OrderStatus status)
        {
            int c = 0;
            foreach (var o in orders) if (o != null && o.Status == status) c++;
            return c;
        }

        static Driver FindMostActiveDriver()
        {
            Driver best = null;
            int max = -1;
            foreach (var d in drivers)
            {
                if (d == null) continue;
                int completed = d.GetCompletedOrders();
                if (completed > max)
                {
                    max = completed;
                    best = d;
                }
            }
            return best;
        }
    }
}
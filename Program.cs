using System;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;
using System.IO;

public class ParkingLot
{
    private static List<ParkingSpotBase> parkingSpots = new List<ParkingSpotBase>();
    private static List<string> checkedOutRecords = new List<string>();
    private const int TotalSpots = 10;
    private const string ParkingSpotsFile = "parking_spots.txt";
    private const string CheckedOutRecordsFile = "checked_out_records.txt";

    public static void Main(string[] args)
    {
        Admin admin = new Admin("delantes", "delantes123");

        bool loggedIn = false;
        while (!loggedIn)
        {
            Console.Clear();
            Console.WriteLine("Welcome to the Parking Lot Management System");
            Console.WriteLine("Admin Login Required");
            Console.Write("Enter username: ");
            string username = Console.ReadLine();
            Console.Write("Enter password: ");
            string password = ReadPassword();

            if (admin.ValidateLogin(username, password))
            {
                Console.WriteLine("\nLogin successful!");
                loggedIn = true;
                Console.WriteLine("Press any key to continue...");
                Console.ReadKey();
            }
            else
            {
                Console.WriteLine("\nInvalid username or password. Please try again.");
                Console.WriteLine("Press any key to retry...");
                Console.ReadKey();
            }
        }

        LoadParkingSpots();
        LoadCheckedOutRecords();

        bool running = true;
        while (running)
        {
            Console.Clear();
            Console.WriteLine("Parking Lot Management System");
            Console.WriteLine("1. View All Spots");
            Console.WriteLine("2. Check-In");
            Console.WriteLine("3. Check-Out");
            Console.WriteLine("4. Search Spot");
            Console.WriteLine("5. Update Check-In Information");
            Console.WriteLine("6. View Sales Report");
            Console.WriteLine("7. Exit");
            Console.Write("Enter your choice: ");

            string choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    ViewAllSpots();
                    break;
                case "2":
                    CheckIn();
                    break;
                case "3":
                    CheckOut();
                    break;
                case "4":
                    SearchSpot();
                    break;
                case "5":
                    UpdateCheckInInformation();
                    break;
                case "6":
                    ViewSalesReport();
                    break;
                case "7":
                    running = false;
                    break;
                default:
                    Console.WriteLine("Invalid choice. Please try again.");
                    Console.ReadKey();
                    break;
            }
        }

        SaveParkingSpots();
        SaveCheckedOutRecords();
    }

    public static string ReadPassword()
    {
        string password = string.Empty;
        while (true)
        {
            var key = Console.ReadKey(true);
            if (key.Key == ConsoleKey.Enter)
            {
                break;
            }
            else if (key.Key == ConsoleKey.Backspace)
            {
                if (password.Length > 0)
                {
                    password = password.Remove(password.Length - 1);
                    Console.Write("\b \b");
                }
            }
            else
            {
                password += key.KeyChar;
                Console.Write("*");
            }
        }
        return password;
    }

    static void LoadParkingSpots()
    {
        parkingSpots.Clear();
        if (File.Exists(ParkingSpotsFile))
        {
            string[] lines = File.ReadAllLines(ParkingSpotsFile);
            foreach (string line in lines)
            {
                string[] parts = line.Split(',');
                if (parts.Length == 4)
                {
                    var spot = new ParkingSpotBase { SpotNumber = int.Parse(parts[0]) };
                    if (!string.IsNullOrEmpty(parts[1]))
                    {
                        IVehicle vehicle = parts[2].ToLower() == "car" ? new Car() : new Motorcycle();
                        vehicle.LicensePlate = parts[1];
                        spot.Vehicle = vehicle;
                        spot.CheckInTime = DateTime.ParseExact(parts[3], "MM/dd/yyyy HH:mm:ss", CultureInfo.InvariantCulture);
                    }
                    parkingSpots.Add(spot);
                }
            }
        }

        if (parkingSpots.Count == 0)
        {
            InitializeParkingSpots();
        }
    }

    static void SaveParkingSpots()
    {
        using (StreamWriter writer = new StreamWriter(ParkingSpotsFile))
        {
            foreach (var spot in parkingSpots)
            {
                if (spot.IsOccupied)
                {
                    writer.WriteLine($"{spot.SpotNumber},{spot.Vehicle.LicensePlate},{spot.Vehicle.GetType().Name},{spot.CheckInTime:MM/dd/yyyy HH:mm:ss}");
                }
                else
                {
                    writer.WriteLine($"{spot.SpotNumber},,,");
                }
            }
        }
    }

    static void LoadCheckedOutRecords()
    {
        if (File.Exists(CheckedOutRecordsFile))
        {
            checkedOutRecords = File.ReadAllLines(CheckedOutRecordsFile).ToList();
        }
    }

    static void SaveCheckedOutRecords()
    {
        if (checkedOutRecords.Count > 0)
        {
            File.AppendAllLines(CheckedOutRecordsFile, checkedOutRecords);
            checkedOutRecords.Clear();
        }
    }

    static void InitializeParkingSpots()
    {
        for (int i = 1; i <= TotalSpots; i++)
        {
            parkingSpots.Add(new ParkingSpotBase { SpotNumber = i });
        }
    }

    static void ViewAllSpots()
    {
        Console.Clear();
        Console.WriteLine("All Parking Spots:");
        Console.WriteLine("==================");

        if (parkingSpots.Count == 0)
        {
            Console.WriteLine("No parking spots available. Please initialize the parking lot.");
        }
        else
        {
            foreach (var spot in parkingSpots)
            {
                string status = spot.IsOccupied ? "Occupied" : "Available";
                Console.WriteLine($"Spot {spot.SpotNumber}: {status}");
                if (spot.IsOccupied)
                {
                    Console.WriteLine($"  License Plate: {spot.Vehicle.LicensePlate}");
                    Console.WriteLine($"  Vehicle Type: {spot.Vehicle.GetType().Name}");
                    Console.WriteLine($"  Check-In Time: {spot.CheckInTime:MM/dd/yyyy HH:mm tt}");
                }
                Console.WriteLine();
            }
        }

        Console.WriteLine("\nPress any key to return to the main menu.");
        Console.ReadKey();
    }

    static void CheckIn()
    {
        Console.Clear();
        Console.WriteLine("Check-In");
        Console.WriteLine("========");

        if (!HasAvailableSpots())
        {
            Console.WriteLine("\nNo available spots. Press any key to return.");
            Console.ReadKey();
            return;
        }

        Console.Write("Enter license plate: ");
        string licensePlate = Console.ReadLine().Trim();
        if (string.IsNullOrEmpty(licensePlate))
        {
            Console.WriteLine("\nInvalid license plate. Press any key to return.");
            Console.ReadKey();
            return;
        }

        Console.Write("Enter vehicle type (car or motorcycle): ");
        string vehicleType = Console.ReadLine().Trim().ToLower();
        if (vehicleType != "car" && vehicleType != "motorcycle")
        {
            Console.WriteLine("\nInvalid vehicle type. Press any key to return.");
            Console.ReadKey();
            return;
        }

        int spotNumber = FindNextAvailableSpot();
        DateTime checkInTime = DateTime.Now;

        IVehicle vehicle = vehicleType == "car" ? new Car() : new Motorcycle();
        vehicle.LicensePlate = licensePlate;
        vehicle.CheckInTime = DateTime.Now;

        var spot = parkingSpots.Find(s => s.SpotNumber == spotNumber);
        spot.Vehicle = vehicle;
        spot.CheckInTime = checkInTime;

        SaveParkingSpots();

        Console.WriteLine($"\nVehicle checked into Spot {spotNumber}.");
        Console.WriteLine($"Check-In Time: {checkInTime:MM/dd/yyyy HH:mm tt}");
        Console.WriteLine("\nPress any key to return.");
        Console.ReadKey();
    }

    static bool HasAvailableSpots()
    {
        return parkingSpots.Any(s => !s.IsOccupied);
    }

    static int FindNextAvailableSpot()
    {
        return parkingSpots.First(s => !s.IsOccupied).SpotNumber;
    }

    static void SearchSpot()
    {
        Console.Clear();
        Console.WriteLine("Search Spot");
        Console.WriteLine("===========");
        Console.Write("Enter spot number to search: ");
        if (int.TryParse(Console.ReadLine(), out int spotNumber))
        {
            var spot = parkingSpots.Find(s => s.SpotNumber == spotNumber);
            if (spot != null)
            {
                Console.WriteLine($"\n{spot}");
            }
            else
            {
                Console.WriteLine("\nSpot not found.");
            }
        }
        else
        {
            Console.WriteLine("\nInvalid input. Please enter a valid number.");
        }

        Console.WriteLine("\nPress any key to return.");
        Console.ReadKey();
    }

    static void UpdateCheckInInformation()
    {
        Console.Clear();
        Console.WriteLine("Update Check-In Information");
        Console.WriteLine("===========================");
        Console.Write("Enter spot number to update: ");
        if (int.TryParse(Console.ReadLine(), out int spotNumber))
        {
            var spot = parkingSpots.Find(s => s.SpotNumber == spotNumber);

            if (spot != null && spot.IsOccupied)
            {
                Console.WriteLine("\nCurrent Check-In Details:");
                Console.WriteLine(spot);
                Console.WriteLine("\nWhat would you like to update?");
                Console.WriteLine("1. License Plate");
                Console.WriteLine("2. Vehicle Type");
                Console.WriteLine("3. Check-In Time and Date");
                Console.Write("Choose option (1, 2, or 3): ");
                string choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        Console.Write("Enter new license plate: ");
                        string newLicensePlate = Console.ReadLine().Trim();
                        if (!string.IsNullOrEmpty(newLicensePlate))
                        {
                            spot.Vehicle.LicensePlate = newLicensePlate;
                            Console.WriteLine("\nLicense plate updated successfully.");
                        }
                        else
                        {
                            Console.WriteLine("\nInvalid license plate. Update cancelled.");
                        }
                        break;
                    case "2":
                        Console.Write("Enter new vehicle type (car or motorcycle): ");
                        string newVehicleType = Console.ReadLine().Trim().ToLower();
                        if (newVehicleType == "car" || newVehicleType == "motorcycle")
                        {
                            spot.Vehicle = newVehicleType == "car" ? new Car() : new Motorcycle();
                            Console.WriteLine("\nVehicle type updated successfully.");
                        }
                        else
                        {
                            Console.WriteLine("\nInvalid vehicle type. Update cancelled.");
                        }
                        break;
                    case "3":
                        Console.Write("Enter new check-in date (MM/dd/yyyy): ");
                        string dateInput = Console.ReadLine();
                        Console.Write("Enter new check-in time (HH:mm): ");
                        string timeInput = Console.ReadLine();
                        Console.Write("Is this AM or PM? ");
                        string amPmInput = Console.ReadLine().Trim().ToUpper();

                        if (DateTime.TryParseExact(dateInput, "MM/dd/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime newDate) &&
                            DateTime.TryParseExact(timeInput, "HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime newTime) &&
                            (amPmInput == "AM" || amPmInput == "PM"))
                        {
                            int hour = newTime.Hour;
                            if (amPmInput == "PM" && hour != 12)
                            {
                                hour += 12;
                            }
                            else if (amPmInput == "AM" && hour == 12)
                            {
                                hour = 0;
                            }

                            DateTime newCheckInTime = new DateTime(newDate.Year, newDate.Month, newDate.Day, hour, newTime.Minute, 0);

                            if (newCheckInTime <= DateTime.Now)
                            {
                                spot.CheckInTime = newCheckInTime;
                                Console.WriteLine("\nCheck-in time and date updated successfully.");
                            }
                            else
                            {
                                Console.WriteLine("\nNew check-in time cannot be in the future. Update cancelled.");
                            }
                        }
                        else
                        {
                            Console.WriteLine("\nInvalid date, time, or AM/PM format. Update cancelled.");
                        }
                        break;
                    default:
                        Console.WriteLine("Invalid choice. Update cancelled.");
                        break;
                }

                SaveParkingSpots();
            }
            else
            {
                Console.WriteLine("\nSpot not found or not occupied.");
            }
        }
        else
        {
            Console.WriteLine("\nInvalid input. Please enter a valid number.");
        }

        Console.WriteLine("\nPress any key to return.");
        Console.ReadKey();
    }

    static void CheckOut()
    {
        Console.Clear();
        Console.WriteLine("Check-Out");
        Console.WriteLine("=========");
        Console.Write("Enter spot number to check-out: ");
        if (int.TryParse(Console.ReadLine(), out int spotNumber))
        {
            var spot = parkingSpots.Find(s => s.SpotNumber == spotNumber);
            if (spot != null && spot.IsOccupied)
            {
                DateTime checkOutTime = DateTime.Now;
                double hoursParked = (checkOutTime - spot.CheckInTime).TotalHours;
                hoursParked = Math.Ceiling(hoursParked);

                decimal rate = spot.Vehicle.GetType().Name.ToLower() == "motorcycle" ? 30 : 50;
                decimal payment = (decimal)hoursParked * rate;

                Console.WriteLine($"\nReceipt:");
                Console.WriteLine($"Spot Number: {spot.SpotNumber}");
                Console.WriteLine($"License Plate: {spot.Vehicle.LicensePlate}");
                Console.WriteLine($"Vehicle Type: {spot.Vehicle.GetType().Name}");
                Console.WriteLine($"Check-In Time: {spot.CheckInTime:MM/dd/yyyy HH:mm tt}");
                Console.WriteLine($"Check-Out Time: {checkOutTime:MM/dd/yyyy HH:mm tt}");
                Console.WriteLine($"Hours Parked: {hoursParked}");
                Console.WriteLine($"Payment Due: P{payment:F2}");

                var salesRecord = new SalesRecord
                {
                    SpotNumber = spot.SpotNumber,
                    Vehicle = spot.Vehicle,
                    CheckOutTime = checkOutTime,
                    HoursParked = hoursParked,
                    Payment = payment
                };
                string record = $"{salesRecord.SpotNumber}|{salesRecord.Vehicle.LicensePlate}|{salesRecord.Vehicle.GetType().Name}|{salesRecord.Vehicle.CheckInTime:MM/dd/yyyy HH:mm:ss}|{salesRecord.CheckOutTime:MM/dd/yyyy HH:mm:ss}|{salesRecord.HoursParked}|{salesRecord.Payment:F2}";
                checkedOutRecords.Add(record);

                spot.Clear();

                SaveParkingSpots();
                SaveCheckedOutRecords();

                Console.WriteLine("\nVehicle checked out successfully.");
            }
            else
            {
                Console.WriteLine("\nSpot not found or not occupied.");
            }
        }
        else
        {
            Console.WriteLine("\nInvalid input. Please enter a valid number.");
        }

        Console.WriteLine("\nPress any key to return.");
        Console.ReadKey();
    }

    static void ViewSalesReport()
    {
        Console.Clear();
        Console.WriteLine("Sales Report");
        Console.WriteLine("============");

        List<SalesRecord> salesRecords = LoadSalesRecords();

        if (salesRecords.Count == 0)
        {
            Console.WriteLine("No sales records found.");
            Console.WriteLine("\nPress any key to return to the main menu.");
            Console.ReadKey();
            return;
        }

        DateTime today = DateTime.Today;
        DateTime startOfWeek = today.AddDays(-(int)today.DayOfWeek);
        DateTime startOfMonth = new DateTime(today.Year, today.Month, 1);
        DateTime startOfYear = new DateTime(today.Year, 1, 1);

        decimal dailySales = salesRecords.Where(r => r.CheckOutTime.Date == today).Sum(r => r.Payment);
        decimal weeklySales = salesRecords.Where(r => r.CheckOutTime >= startOfWeek).Sum(r => r.Payment);
        decimal monthlySales = salesRecords.Where(r => r.CheckOutTime >= startOfMonth).Sum(r => r.Payment);
        decimal yearlySales = salesRecords.Where(r => r.CheckOutTime >= startOfYear).Sum(r => r.Payment);

        Console.WriteLine($"Daily Sales (Today): P{dailySales:F2}");
        Console.WriteLine($"Weekly Sales (This Week): P{weeklySales:F2}");
        Console.WriteLine($"Monthly Sales (This Month): P{monthlySales:F2}");
        Console.WriteLine($"Yearly Sales (This Year): P{yearlySales:F2}");

        Console.WriteLine("\nPress any key to return to the main menu.");
        Console.ReadKey();
    }

    static List<SalesRecord> LoadSalesRecords()
    {
        List<SalesRecord> salesRecords = new List<SalesRecord>();

        if (File.Exists(CheckedOutRecordsFile))
        {
            HashSet<string> uniqueRecords = new HashSet<string>(File.ReadAllLines(CheckedOutRecordsFile));

            foreach (string line in uniqueRecords)
            {
                string[] parts = line.Split('|');
                if (parts.Length == 7)
                {
                    IVehicle vehicle = parts[2].ToLower() == "car" ? new Car() : new Motorcycle();
                    vehicle.LicensePlate = parts[1];
                    vehicle.CheckInTime = DateTime.ParseExact(parts[3], "MM/dd/yyyy HH:mm:ss", CultureInfo.InvariantCulture);

                    salesRecords.Add(new SalesRecord
                    {
                        SpotNumber = int.Parse(parts[0]),
                        Vehicle = vehicle,
                        CheckOutTime = DateTime.ParseExact(parts[4], "MM/dd/yyyy HH:mm:ss", CultureInfo.InvariantCulture),
                        HoursParked = double.Parse(parts[5]),
                        Payment = decimal.Parse(parts[6])
                    });
                }
            }
        }

        return salesRecords;
    }
}

public interface IUser
{
    bool ValidateLogin(string username, string password);
}

public abstract class User : IUser
{
    public string Username { get; set; }
    protected string Password { get; set; }

    protected User(string username, string password)
    {
        Username = username;
        Password = password;
    }

    public abstract bool ValidateLogin(string username, string password);
}

public class Admin : User
{
    public Admin(string username, string password) : base(username, password) { }

    public override bool ValidateLogin(string username, string password)
    {
        return Username == username && Password == password;
    }
}

public interface IVehicle
{
    string LicensePlate { get; set; }
    DateTime CheckInTime { get; set; }
}

public class Car : IVehicle
{
    public string LicensePlate { get; set; }
    public DateTime CheckInTime { get; set; }
}

public class Motorcycle : IVehicle
{
    public string LicensePlate { get; set; }
    public DateTime CheckInTime { get; set; }
}

public interface IParkingSpotBase
{
    int SpotNumber { get; set; }
    IVehicle Vehicle { get; set; }
    DateTime CheckInTime { get; set; }
    bool IsOccupied { get; }
    void Clear();
}

public class ParkingSpotBase : IParkingSpotBase
{
    public int SpotNumber { get; set; }
    public IVehicle Vehicle { get; set; }
    public DateTime CheckInTime { get; set; }

    public bool IsOccupied => Vehicle != null && !string.IsNullOrEmpty(Vehicle.LicensePlate);

    public void Clear()
    {
        Vehicle = null;
        CheckInTime = default;
    }

    public override string ToString()
    {
        if (IsOccupied)
        {
            return $"Spot Number: {SpotNumber} | License Plate: {Vehicle.LicensePlate} | Vehicle Type: {Vehicle.GetType().Name} | Check-In Time: {CheckInTime:MM/dd/yyyy HH:mm tt}";
        }
        else
        {
            return $"Spot Number: {SpotNumber} | Status: Available";
        }
    }
}

public interface ISalesRecord
{
    int SpotNumber { get; set; }
    IVehicle Vehicle { get; set; }
    DateTime CheckOutTime { get; set; }
    double HoursParked { get; set; }
    decimal Payment { get; set; }
}

public class SalesRecord : ISalesRecord
{
    public int SpotNumber { get; set; }
    public IVehicle Vehicle { get; set; }
    public DateTime CheckOutTime { get; set; }
    public double HoursParked { get; set; }
    public decimal Payment { get; set; }
}


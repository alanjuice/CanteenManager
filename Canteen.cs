using Microsoft.Data.Sqlite;
using ConsoleTables;

namespace CanteenLogic
{
    class Canteen
    {
        //TODO

        //1. Add exception handling to database operations
        //2. Create a better order menu - Multi Item Buying & Bill Generation
        //3. Make database updates at the end of order instead of after each product so that user can cancel
        //   in between an order and no change is reflected on the database
        //4. Make a cart system
        //5. Make GUI with WPF
        //6. 

        static string connectionString = "Data Source=store.db";

        public void StartMenu()
        {
            createDb();
            bool running = true;
            while (running)
            {
                Console.WriteLine(@"
                MENU
                1.Make an order
                2.Add new item
                3.Add stocks
                4.Exit
                Enter the choice ");
                string input = Console.ReadLine();
                switch (input)
                {
                    case "1":
                        makeOrder();
                        break;
                    case "2":
                        addItem();
                        break;
                    case "3":
                        addStocks();
                        break;
                    case "4":
                        running = false;
                        break;
                    default:
                        Console.WriteLine("Invalid choice");
                        break;
                }

            }
        }


        void createDb()
        {
            //Creates the database if not exists
            updateDb("create table if not exists items(item_id char(2) primary key,name varchar(20), quantity int, price float)");
        }


        void addItem()
        {
            //To add new item to the system
            //Accepts the neccessary details from user and adds a correspoding row in the database

            using (SqliteConnection connection = new SqliteConnection(connectionString))
            {

                Console.WriteLine("Enter the item id(I--):");
                string item_id = Console.ReadLine();
                Console.WriteLine("Enter the item name:");
                string name = Console.ReadLine();
                Console.WriteLine("Enter the quantity:");
                int quantity = Convert.ToInt32(Console.ReadLine());
                Console.WriteLine("Enter the price of one item:");
                int price = Convert.ToInt32(Console.ReadLine());
                string query = $"insert into items values('{item_id}','{name}',{quantity},{price})";

                connection.Open();
                SqliteCommand command = connection.CreateCommand();
                command.CommandText = query;
                command.ExecuteNonQuery();
                connection.Close();
                Console.WriteLine("Item added successfully");
            }
        }



        void makeOrder()
        {
            //Prints the available items to the user
            //Accepts orders from user

            bool stillBrowsing = true;
            List<(string, int, float)> orders = new List<(string, int, float)>();


            while (stillBrowsing)
            {
                
                var items = getItems();

                var menu = new ConsoleTable("ID", "NAME", "QUANTITY", "PRICE");
                foreach (var item in items)
                {
                    menu.AddRow(item[0], item[1], item[2], item[3]);
                }
                menu.Write(Format.Minimal);
                Console.WriteLine("What would you like to order(Enter the ID)\n(Type E to close your order)");
                string food_id = Console.ReadLine();

                if (food_id.ToLower() == "e")
                {
                    float totalAmount = 0;
                    stillBrowsing = false;
                    Console.WriteLine("Bill \n\n");
                    var orderMenu = new ConsoleTable("Name", "Quantity","Total Price");
                    foreach (var item in orders)
                    {                       
                        orderMenu.AddRow(item.Item1, item.Item2, item.Item2*item.Item3);
                        totalAmount += item.Item2 * item.Item3;
                    }
                    orderMenu.Write(Format.Minimal);
                    Console.WriteLine($"\nTotal Amount : {totalAmount}");
                    continue;
                }

                Boolean itemFound = false;

                foreach (var item in items)
                {

                    if (item[0] == food_id)
                    {
                        itemFound = true;
                        Console.WriteLine($"How many {item[1]} would you like to order");
                        int quantity = Convert.ToInt32(Console.ReadLine());
                        try
                        {
                            if (Convert.ToInt32(item[2]) >= quantity)
                            {
                                orders.Add((item[1], quantity, float.Parse(item[3])));
                                updateDb($"update items set quantity=quantity-{quantity} where item_id='{food_id}'");
                                Console.WriteLine("Ordered Successfully");

                            }
                            else
                            {
                                Console.WriteLine("Sorry, We dont have that much available");
                            }
                        }
                        catch (System.FormatException)
                        {
                            Console.WriteLine("Please input an integer value for quantity");
                            return;
                        }

                        
                        
                        break;
                    }
                }

                if (!itemFound)
                {
                    Console.WriteLine("Invaid Product ID");
                }

            }

        }

        List<string[]> getItems()
        {
            //  Returns a List of item which are available in the database
            // Each item consists of id,name,price,quantity

            using (SqliteConnection connection = new SqliteConnection(connectionString))
            {
                List<string[]> items = new List<string[]>();

                connection.Open();
                SqliteCommand command = connection.CreateCommand();
                command.CommandText = "select * from items";
                var reader = command.ExecuteReader();


                while (reader.Read())
                {
                    string[] item = {
                        reader.GetString(0),    //Id
                        reader.GetString(1),    //Name
                        reader.GetString(2),    //Quantity
                        reader.GetString(3),    //Price
                    };
                    items.Add(item);
                }
                connection.Close();
                return items;
            }
        }

        void addStocks()
        {
            //Add more items to the stocks
            //Increase the quantity of existing item in database


            Console.WriteLine("Enter the PID you want to update:");
            string pid = Console.ReadLine();
            Console.WriteLine("Enter the quantity you want to add:");

            try
            {
                int quantity = Convert.ToInt32(Console.ReadLine());
                int value = updateDb($"update items set quantity = quantity+{quantity} where item_id='{pid}'");
                if (value == 0)
                {
                    Console.WriteLine("Invalid Product ID");
                }
            }
            catch (System.FormatException)
            {
                Console.WriteLine("Please input an integer value for quantity");
                return;
            }
            
            //No database change when invalid/unexisting PID is given
            
        }

        int updateDb(string query)
        {
            //Method to make update queries to the database

            using (SqliteConnection connection = new SqliteConnection(connectionString))
            {
                connection.Open();
                SqliteCommand command = connection.CreateCommand();
                command.CommandText = query;
                int noOfRowsUpdated = command.ExecuteNonQuery();
                connection.Close();
                return noOfRowsUpdated;

            }
        }
    }
}

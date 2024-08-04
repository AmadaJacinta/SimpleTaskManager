using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace SimpleTaskManager
{
    // Class representing a task
    public class Task
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsCompleted { get; set; } = false;

        // Default constructor for JSON deserialization
        public Task() { }

        // Constructor for creating a task with only a name
        public Task(string name)
        {
            Name = name;
            Description = "";
        }

        // Constructor for creating a task with a name and description
        public Task(string name, string description)
        {
            Name = name;
            Description = description;
        }
    }

    // Main program class
    public static partial class Program
    {
        private const string DataFileName = "tasks.json"; // File name for storing task data

        public static void Main(string[] args)
        {
            string? input;
            Dictionary<int, Task> taskList = LoadTasks();
            int nextTaskId = GetNextTaskId(taskList);

            while (true)
            {
                Console.Write("> ");
                input = Console.ReadLine();

                if (string.IsNullOrEmpty(input))
                {
                    Console.WriteLine("Type \"help\" for usage information.");
                    continue;
                }

                string[] parts = SplitInput(input);

                switch (parts[0].ToLower())
                {
                    case "add":
                        HandleAddCommand(parts, taskList, ref nextTaskId);
                        break;

                    case "list":
                        HandleListCommand(taskList);
                        break;

                    case "complete":
                        HandleCompleteCommand(parts, taskList);
                        break;

                    case "delete":
                        HandleDeleteCommand(parts, taskList);
                        break;

                    case "edit":
                        HandleEditCommand(parts, taskList);
                        break;

                    case "exit":
                        SaveTasks(taskList);
                        return;

                    case "help":
                        HandleHelpCommand();
                        break;

                    default:
                        Console.WriteLine("Invalid command. Type \"help\" for usage information.");
                        break;
                }
            }
        }

        // Handle the 'add' command
        private static void HandleAddCommand(string[] parts, Dictionary<int, Task> taskList, ref int nextTaskId)
        {
            if (parts.Length == 2)
            {
                taskList.Add(nextTaskId, new Task(parts[1]));
                Console.WriteLine($"Task \"{parts[1]}\" added with ID {nextTaskId}");
                nextTaskId++;
            }
            else if (parts.Length == 3)
            {
                taskList.Add(nextTaskId, new Task(parts[1], parts[2]));
                Console.WriteLine($"Task \"{parts[1]}\" added with ID {nextTaskId}");
                nextTaskId++;
            }
            else
            {
                Console.WriteLine("Invalid usage of 'add'. Usage: add \"task_name\" [\"description\"]");
            }

            SaveTasks(taskList);
        }

        // Handle the 'list' command
        private static void HandleListCommand(Dictionary<int, Task> taskList)
        {
            if (taskList.Count == 0)
            {
                Console.WriteLine("No tasks in the list.");
            }
            else
            {
                foreach (KeyValuePair<int, Task> task in taskList)
                {
                    Console.WriteLine($"{task.Key}. [{(task.Value.IsCompleted ? "x" : " ")}] {task.Value.Name}: {task.Value.Description}");
                }
            }
        }

        // Handle the 'complete' command
        private static void HandleCompleteCommand(string[] parts, Dictionary<int, Task> taskList)
        {
            if (parts.Length == 2 && int.TryParse(parts[1], out int taskId))
            {
                if (taskList.ContainsKey(taskId))
                {
                    taskList[taskId].IsCompleted = !taskList[taskId].IsCompleted;
                    Console.WriteLine($"Status of task {taskId} changed.");
                }
                else
                {
                    Console.WriteLine($"Task with ID {taskId} not found.");
                }
            }
            else
            {
                Console.WriteLine("Invalid usage of 'complete'. Usage: complete <task_id>");
            }

            SaveTasks(taskList);
        }

        // Handle the 'delete' command
        private static void HandleDeleteCommand(string[] parts, Dictionary<int, Task> taskList)
        {
            if (parts.Length == 2 && int.TryParse(parts[1], out int taskId))
            {
                if (taskList.ContainsKey(taskId))
                {
                    taskList.Remove(taskId);
                    Console.WriteLine($"Task {taskId} deleted.");
                }
                else
                {
                    Console.WriteLine($"Task with ID {taskId} not found.");
                }
            }
            else
            {
                Console.WriteLine("Invalid usage of 'delete'. Usage: delete <task_id>");
            }

            SaveTasks(taskList);
        }

        // Handle the 'edit' command
        private static void HandleEditCommand(string[] parts, Dictionary<int, Task> taskList)
        {
            if (parts.Length >= 3 && int.TryParse(parts[1], out int taskId))
            {
                if (taskList.ContainsKey(taskId))
                {
                    Task task = taskList[taskId];

                    string newName = parts.Length > 2 ? parts[2] : task.Name;
                    string newDescription = parts.Length > 3 ? parts[3] : task.Description;

                    task.Name = newName;
                    task.Description = newDescription;

                    Console.WriteLine($"Task {taskId} updated.");
                }
                else
                {
                    Console.WriteLine($"Task with ID {taskId} not found.");
                }

                SaveTasks(taskList);
            }
            else
            {
                Console.WriteLine("Invalid usage of 'edit'. Usage: edit <task_id> \"new_name\" [\"new_description\"]");
            }
        }

        // Handle the 'help' command
        private static void HandleHelpCommand()
        {
            Console.WriteLine("Available commands:");
            Console.WriteLine("  add \"task_name\" [\"description\"] - Add a new task");
            Console.WriteLine("  list - List all tasks");
            Console.WriteLine("  complete <task_id> - Mark a task as complete");
            Console.WriteLine("  delete <task_id> - Delete a task");
            Console.WriteLine("  edit <task_id> \"new_name\" [\"new_description\"] - Edit a task");
            Console.WriteLine("  help - Show this help");
            Console.WriteLine("  exit - Exit from application");
        }

        // Split input string into parts
        private static string[] SplitInput(string input)
        {
            MatchCollection matches = Regex.Matches(input, @"""(.*?)""|(\S+)");
            string[] result = new string[matches.Count];

            for (int i = 0; i < matches.Count; i++)
            {
                result[i] = matches[i].Groups[1].Success ? matches[i].Groups[1].Value : matches[i].Groups[2].Value;
            }

            return result;
        }

        // Load tasks from file
        private static Dictionary<int, Task> LoadTasks()
        {
            try
            {
                if (File.Exists(DataFileName))
                {
                    string json = File.ReadAllText(DataFileName);
                    return JsonSerializer.Deserialize<Dictionary<int, Task>>(json)!;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading tasks: {ex.Message}");
            }

            return new Dictionary<int, Task>();
        }

        // Save tasks to file
        private static void SaveTasks(Dictionary<int, Task> taskList)
        {
            try
            {
                string json = JsonSerializer.Serialize(taskList);
                File.WriteAllText(DataFileName, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving tasks: {ex.Message}");
            }
        }

        // Get the next available task ID
        private static int GetNextTaskId(Dictionary<int, Task> taskList)
        {
            int maxId = 0;
            foreach (int id in taskList.Keys)
            {
                if (id > maxId)
                {
                    maxId = id;
                }
            }
            return maxId + 1;
        }
    }
}
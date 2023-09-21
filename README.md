# Mithril
Mithril is a Programming Language based on Javascript syntax

test.txt
```cs
﻿// Coming Soon:
// import("foo.txt") // run a file

// Function for easy logging
function Log(value) {
    // Built-In Global Function
    // print(format, ...args)
    print("[LOGGER]: {0}", value);

    // Returns in this language are implicit always
    // So the last function call or value will be returned
    null
}
// Send a Log using the Function Above
Log("This is an example log");
// Example of a constructor
function CreatePerson(name, age) {
    // Implicit return of an Object Literal
    { name, age, type: "Person" }
}
// Use constructor to make a new const person
const person = CreatePerson("Glatrix", 99);
// Log obj.prop
print(person.name);
// set value to obj.prop
person.name = "Bob";
// print changed obj.prop
print(person.name);
// Use some string formatting
print("{0} is {1} years old!", person.name, person.age)

print("Math!! '5 * (6 - 3) / 2' = {0}", 5 * (6 - 3) / 2);

```

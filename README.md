# About
KappaView, previously known as STGResourceReader, is a handy tool for reading memory using customizable profiles. This is similar to [RealTimeDRCPointsDisplayer](https://github.com/hoangcaominh/RealTimeDRCPointsDisplayer) but can support any game outside of the Touhou Project series, even non-games as well if a corresponding profile is available.

---
## Creating a profile

A profile is basically a `JSON` file containing the information to tell KappaView what memory to be read from the process. Optionally a profile can have `Lua` files as "modules" to allow KappaView to do more stuffs than just reading the memory. The name of these files can be whatever you want but it's recommended to use names like how you name variables in programming. These files should be placed in the `profiles` folder  located in the same folder with the application. Additionally you can have Lua files in `profiles`'s subfolders and profiles can use relative paths to find them. The JSON object in a profile is implicitly divided into 2 sections, `Info` and `Variable`.

### Info

This section has 4 pieces of information need to be filled: 

- `Version (string)`: The version of the configuration file. Not really necessary, so you can fill anything to indicate the version of your profile that follows the `x.y.z.t` format, where x, y, z, t are numeric.
- `Target (string)`: The name of the process (game) you are targeting.
- `Platform (number)`: The platform the targeted process. This application is made to read memory from Windows applications only so the number to be filled will be either `32` (32-bit) or `64` (64-bit) (You can google how to determine whether a process is a 32-bit or 64-bit application).

##### The object "Reset"
`Reset` is a special object and is not required to be included. It is used to set all the counters (which will also be mentioned later) value to 0 and it has only 1 key:

`Parameters`: An array of strings which are the name of the variables defined in the below section. What it does is its function, which must be declared in the Lua file as `Reset`, takes the value read as parameters, executes and returns a boolean value to trigger the "Reset" function (if `true`, not otherwise).

### Variable

Unlike the previous section where you must fill all the information, you can leave this one empty, which will result in the application printing nothing. This section may consist of one or more JSON objects, representing the variables you want to either read or create. Again you can name these whatever you want but it's recommended to name like how you name variables in programming. The order you define variables here is the order the status window will print them. Below is a list of keys in a variable's value (JSON object):

- `Name (string)`: The name of the variable to be displayed on the status window.
- `Type (string)`: Type of the value of this variable. There are 3 types used:
	- `Static`: The variable is read from the target and directly displayed on the window.
	- `Counter`: The variable is read from the target and an associated counter will increment whenever its function returns `true`. The counter function in the Lua file takes 2 parameters, generally called "prev" and "curr" and returns a boolean value indicating the distance of these two variables.
	- `Custom`: The variable receives the return value from the Lua function defined by the user. This is usually used for custom calculations like DRC score, ISCORE, etc..
- `DataType (string)`: The data type of the variable. This one MUST be defined carefully as inaccurate data type will cause KappaView to read the value with missing or extra bytes, hence inaccurate results (not strictly applied for "Custom" type). The variable data type is one of the following:
    - `Bool`: Representing a Boolean (`true` or `false`) value.
    - `Int8`: Representing an 8-bit signed integer (1 byte).
    - `UInt8`: Representing an 8-bit unsigned integer (1 byte).
    - `Int16`: Representing a 16-bit signed integer (2 bytes).
    - `UInt16`: Representing a 16-bit unsigned integer (2 bytes).
    - `Int32`: Representing a 32-bit signed integer (4 bytes).
    - `UInt32`: Representing a 32-bit unsigned integer (4 bytes).
    - `Int64`: Representing a 64-bit signed integer (8 bytes).
    - `UInt64`: Representing a 64-bit unsigned integer (8 bytes).
    - `Float`: Representing a real number (4 bytes).
    - `Double`: Representing a real number (8 bytes).
    - `Char`: Representing an ASCII character (1 byte).

    You can declare an array of values by adding a colon (`:`) followed by a value indicating the size of the array after the data type. For example an array of 3 boolean values: `"DataType": "Bool:3"`.
- `Address (array of strings)`: The address of the value to be read. The first address is the base (static) address. If the value's address is not static; it's either dynamic or a multiple-level pointer, append the offset of the pointer to the array till the base address with all the pointer offsets eventually points to the final address. All string values in this array MUST be written in hexadecimal format like `"004BCDEF"`.
- `Parameters (array of strings)`: (Only applies to "Custom" type) The parameters its function takes in the Lua file. Similar to the one in the `Reset` function above.
- `Display (JSON object)`: Alternative values to be displayed if the value equals to one of the keys specified in this object, empty object results in raw value to be printed out. If this is not defined then the value is hidden and not printed on the status window.

### Modules
SImilar to many programming languages, you can include Lua files as "modules" in your profile. All of the functions needed in the profile are included through this method. Lua is easy-to-learn and well-supported so you can actually do lots of stuffs in this file. You can even include other lua files by using the `require` keyword but it's another story. I recommend having a look at Lua tutorial if you have no clue how to write Lua codes, it shouldn't take long.
NOTE: A Lua function can be overrided by the order of the files to be included in `LuaImport` if there are more than 1 definition of it.

## Sample
This is a simple example to create profile for Wily Beast and Weakest Creature. Keep in mind that double slash is NOT a comment line in JSON file format.

##### th17.json
```javascript
{
    "Version": "1.0",   // Profile version 1.0
    "Target": "th17",  // The execution name for Wily Beast and Weakest Creature
    "Platform": 32, // WBaWC is a 32-bit application
	"Reset": {
		"Parameters": ["frame_count"]	// The Reset function takes the value of "frame_count" as a parameter
	},
	"frame_count": {	// Frame counter in a stage.
		"Name": "Frame count",
		"Type": "Static",	// The type of the variable. Here we want to read the raw value
		"DataType": "UInt32",	// The size of frame counter in the game is 4 bytes
		"Address": ["004B59E8"]    // The base address of the variable. This is a static address so no offsets are needed
	},
    "miss": {  // Miss variable. Actually this property reads the life variable in the game but we define this variable's type as counter so we can determine how many times the player loses a life
        "Name": "Misses",
        "Type": "Counter",  // The type of the variable. Here we want to count the misses
        "DataType": "UInt8",
        "Address": ["004B5A40"],
		"Display": {}	// We want to print the raw number of miss count
    },
    "difficulty": { // Difficulty variable
        "Name": "Difficulty",
        "Type": "Static",
        "DataType": "UInt8",
        "Address": ["004B5A00"],
        "Display": {   // Each difficulty has one distinct value and they are defined in the Display object
            "0": "Easy",
            "1": "Normal",
            "2": "Hard",
            "3": "Luantic",
            "4": "Extra"
		}
    },
	"score_mem": {	// Abbreviation for score memory (score variable in the game)
		"Name": "Score",
		"Type": "Static",
		"DataType": "UInt32",
		"Address": ["004B59FC"]
	},
	"continue": {
		"Name": "Continue",
		"Type": "Static",
		"DataType": "UInt8",
		"Address": ["004B5A04"]	// See "frame_count", "score_mem" and this variable have "Dislpay" not defined? That's because we don't want them to be printed on the status window (unless you are debugging the values)
	},
	"score_out": {	// Score. The actual score in the game is the raw score multipled by 10 plus the number of continues
		"Name": "Score",
		"Type": "Custom",	// Custom type. We want to calculate the total (actual) score instead of reading the memory
		"DataType": "UInt64",
		"Parameters": ["score_mem", "continue"],	// The function takes the value of "score_mem" and "continue" as parameters
		"Display": {}
	}
}
```

##### default.lua
```lua
-- This is a comment line.
--[[ This is a comment block
function default_trigger(prev, curr)
	return prev ~= curr
end
]]
function Reset(frame_count)
	return frame_count < 0x08	-- Returns true if frame_count is smaller than 8 (0x08 in hexadecimal)
end
```

##### th17.lua
```lua
function miss(prev, curr)
	return prev - curr == 1
end

function score_out(score, continue)
	return score * 10 + continue	-- Returns the actual score in the game
end
```

---
## Contribution

You can help us improve this application by creating a new issue on this repository or DM to this Discord user: `Cao Minh#1185`

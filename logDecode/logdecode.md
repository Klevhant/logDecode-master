# LogDecode C#
## Function
* Decode Bike Log (binary) to CSV File and C# DataTable
  * U = unsighed integer, ~~only for 1,2,4,8 bytes~~
  * S = signed intger, ~~only for 1,2,4,8 bytes~~
  * B = binary
  * H = hex
  * C = char (string), reversed byte order
  * D = char (string), normal byte order 

## EEPROM map
* format = `<Name>/<Type>/<Len>/<Var>/<Base>/<Desc>;`
* `<Name>` = Variable Name, should be alpha-numeric and underline _
  * ** **each name should be different**
* `<Type>` = num		-- not used, to be removed ??
* `<Len>` = how many bytes
* `<Var>` = variable type to decode
  * U,S,B,H,F,C,D 	-- as above
* `<Base>` = multiplier
  * ** **only applied for U,S,F**
* `<Desc>` = long description             

## Todo 
* padding zero to match the result from AutoHotKey
* Transpose output for **ID**
* F = float
* to load different Stru definition (or auto detect)
  * 6AAJ @ **VCU_SOFTWARE_VERSION	c0~c3**
  * VCU 1.0 ...
* to handle non-ASCII char

## Version History
### 180810
* add RTC column header
```cs
outHeading += ",Y,M,D,HH,MM,SS";
```
* ExcelDb.cs => roll back to `Microsoft.Jet.OleDb.4.0`
#### EEPROM MAP
* Err result missing `MAIN_CNT_LOW` => duplicate name
* add `EEPROM MAP (VCU2.0) 180721_6AAJ - KSOC.xls`
### 171017
* ExcelDb.cs => `Microsoft.Ace.OleDb.12.0` for database driver issue
### 151125
* decode 32bit-RTC
* support non power-of-2 (1,2,4,8) integer
* only write row when hasData = true;
* add EEPROM Map dropdownlist
* Start, End defines in Excel
* tested ID, Err, Log
### 151124
* logStru can output CSV, tested for logStru (almost OK)
### 151123
* porting from AutoHotKey (5.11 -- 150723) to C#

## C#
* `EEPROM*.xls` in `EEPROM MAP` (source code folder) will overwrite that in `bin` folder
* `loadMapList()` => get `EEPROM*.xls`
* `btnOpenFile_Click()` main decoding logic
* VCU 2.0 32-bit RTC decoded from `RTC0_G_Y_ANG`, `RTC1_G_X_ANG`, `RTC2_K_BAT_AD`, `RTC3_K_AVG_TQ` (hard-code)


## AutoHotKey
* #339 initStru
* #347 		GoSub init%StruName%
	*	length := %StruName%Len
* #390 LogStru
```
LogStruRegEx := "(?P<Name>[^/]+)/(?P<Type>[^/]*)/(?P<Len>[^/]*)/(?P<Var>[^/]*)/(?P<Base>[^/]*)/(?P<Desc>[^/]*)(?P<End>;+)"
```	

* #535 DecodeStru		;U,S,B,H,F,C,D  		
* #665 dumpStru

# APIDataSyncXMLGenerator

## Overview
APIDataSyncXMLGenerator is a .NET Core console application that automates data retrieval from an API, updates a database, and generates an XML file. It processes product codes from a `.txt` file, retrieves corresponding IDs from the database, fetches detailed product data via API, and stores the results in both the database and an XML file.

## Features
- Reads product codes from an input `.txt` file
- Queries the database to obtain product IDs
- Sends API requests to fetch product details
- Updates the database with retrieved data
- Generates an XML file containing all fetched information
- Supports MySQL database
- Works with REST API

## Requirements
- .NET Core SDK
- MySQL or PostgreSQL database
- API access credentials

## Installation
1. Clone the repository:
   ```sh1
   git clone https://github.com/your-repo/APIDataSyncXMLGenerator.git
   ```
2. Navigate to the project directory:
   ```sh1
   cd APIDataSyncXMLGenerator
   ```
3. Install dependencies:
   ```sh1
   dotnet restore
   ```
4. Build the project:
   ```sh1
   dotnet build
   ```

## Usage
1. Prepare the input file `input/products.txt` with product codes (one per line).
2. Configure the database and API settings in `appSettings.xml`.
3. Run the application:
   ```sh1
   dotnet run
   ```
4. The output XML file will be generated in the `output/` folder.

## Configuration
Modify `App.config` to set up database connection strings and API details:

```xml
<appSettings>
    <!-- Config parameters -->
    <add key="ApiFetchIntervalSeconds" value="" />
    <add key="Supplier" value=""/>
    
    <!-- Database credentials -->
    <add key="DbUsername" value="" />
    <add key="DbPassword" value="" />
    <add key="DbName" value="" />
    <add key="DbIp" value="" />
    
    <!-- Database Table Names -->
    <add key="TableProducts" value="" />
    <add key="TableApplications" value="" />
    <add key="TableCrossNumbers" value="" />
    <add key="TableFiles" value="" />
    <add key="TableImages" value="" />
    <add key="TablePackages" value="" />
    <add key="TableParameters" value="" />
    
    <!-- Gaska API credentials -->
    <add key="ApiAcronym" value="" />
    <add key="ApiPerson" value="" />
    <add key="ApiPassword" value="" />
    <add key="ApiKey" value="" />
    <add key="ApiBaseUrl" value="" />
</appSettings>
```

## License
This project is licensed under the MIT License.

# Active Query Builder Angular demo

##### To get Active Query Builder embedded in your Angular app, do the followng: 

1. Copy the `aqb.client.js` file to the "assets" folder.

2. Link this script as global in the `.angular-cli.json` file: 

        "scripts": ["./assets/aqb.client.js"]
    
3. Declare "AQB" as a global variable in the `src/typings.d.ts` file:

        declare var AQB: any;
        
4. Create the "querybuilder" component and put the Active Query Builder HTML markup and the initialization code into it. See the sample in the `src/app/querybuilder` file.

##### To run the project:

1. Execute the `npm install` command,

2. Execute the `npm run build` command,

3. Run the JavaScript demo project,

4. Open the Angular Demo page.

---
The project was made using the `ng new ProjectName` command accorting to the [Angular Quick Start Guide](https://angular.io/guide/quickstart).

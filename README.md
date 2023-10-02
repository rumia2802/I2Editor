# I2Language Editor
This projects is efforts at modding custom languages into games which use the [I2Localization](https://assetstore.unity.com/packages/tools/localization/i2-localization-14884) plugin. The project is currently being developed for the game [Vampire Survivors](https://store.steampowered.com/app/1794680/Vampire_Survivors/), but probably can be used for other games as well which use the same plugin.

## Features implemented
- Importing/exporting raw `i2language.dat` files.
- Storing everything as JSON files.
- Import/export to CSV files (can be used for Google Sheets).

## How to use
1. Extract the `i2language.dat` file from game's `resources.assets` file using [Unity Assets Bundle Extractor](https://github.com/SeriousCache/UABE). (Use `Export Raw` option)
2. Launch the program and import the extracted `i2language.dat` file.
3. Then, save the project to a folder.
4. If you're making changes to the files manually, you'll have to re-open the project to export. If you're using the import feature, you can just save the project and export it.

## How to add new languages
1. Open the `languages.json` file in a text editor.
2. Add a new entry to the array. The entry should look like this:
```json
{
    "Name": "en",
    "Code": "en",
    "Flags": 0
}
```
3. Save the file and open `locales` folder which was created after exporting the files.
4. Copy-paste any of the existing language folders and rename it to the language code you specified in the `languages.json` file.
5. Edit the file in any way you want.
6. Launch the program, select `Export` option and pick the folder where you exported the files.
7. After that, you will get a new `i2language.dat` file which you can put back into the game's `resources.assets` file using UABE.

## Using CSV files
1. Open any project.
2. Select *Export* -> *Export terms as .csv file*
3. Open the exported file in Google Sheets.
4. Make changes to the file.
5. Export the file back to CSV format. (Important: make sure to delete extra columns which were added by Google Sheets)
6. Select *Import* -> *Import terms from .csv file*
7. Save the project and export it.

## License
This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Contact
If you have any questions, you can contact me on Discord: `@prevter`
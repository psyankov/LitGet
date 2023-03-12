# LitGet

**\*\*\*\*\* WORK IN PROGRESS \*\*\*\*\***

LitGet command line application collects data and download files for the books purchased by the user at the e-book store https://www.litres.ru/

LitGet relies on the Selenium Web Driver to launch Chrome browser and to navigate the site, therefore it is dependent on specific names of the web page elements at litres.ru, which may change in future, making the application fragile. Unfortunately, to my knowledge, Litres does not provide an official public API for its service. If anyone reading this has different information, please let me know!

Default location of the book data (stored as json) and downloaded files is LitGet folder in the user's default Documents folder. Edit App.config to change this location. On first invocation the user will be prompted for the login info for litres.ru unless already entered in App.config.

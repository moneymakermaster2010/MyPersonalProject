This project is setup to use a SQLEXPRESS database in
the App_Data folder. 

If you do not have Microsoft SQL Server Express installed, and
instead have a Microsoft SQL Server running, execute the 
moviereviews.sql script included in the App_Data folder. 

The script will create a new database (moviereviews), 
add two tables (movies and reviews), then populate the 
tables with sample data. 

You'll also need to open the web.config file and change the 
MovieReviewEntities connection string to point to the server 
where you created the database (change DataSource=<yourserver>). 

The template modified in the article is the 
Models\Default\MoviesDefault.tt template. The code generated 
from this template is the code used for data access in the 
application. 
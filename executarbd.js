
import mysql from 'mysql2/promise';

// Create the connection to database
const connection = await mysql.createConnection({
  host: 'localhost',
  user: 'root',
  database: 'erp_bd',
});
try {
  const [results, fields] = await connection.query(
    'SELECT * FROM `user` '
  );

  console.log(results); // results contains rows returned by server
  console.log(fields); // fields contains extra meta data about results, if available
  console.log('conctou com o banco de dados');
} catch (err) {
  console.log(err);
}

  $SQLServer = "CO1EBSSCMPSQL03"
 $SQLDBName = "SQLInfo"
 $uid ="FAREAST\v-esde"
 $pwd = "Infy@123417##@"
 $SqlQuery = "SELECT top 100 * from SCMAPISL
 $SqlConnection = New-Object System.Data.SqlClient.SqlConnection
 $SqlConnection.ConnectionString = "Server = $SQLServer; Database = $SQLDBName; User ID = $uid; Password = $dpwd; trusted_connection = false;"
 $SqlCmd = New-Object System.Data.SqlClient.SqlCommand
 $SqlCmd.CommandText = $SqlQuery
 $SqlCmd.Connection = $SqlConnection
 $SqlAdapter = New-Object System.Data.SqlClient.SqlDataAdapter
 $SqlAdapter.SelectCommand = $SqlCmd
 $DataSet = New-Object System.Data.DataSet
 $SqlAdapter.Fill($DataSet)
 $DataSet.Tables[0] | out-file "D:\Eswar\data.txt";
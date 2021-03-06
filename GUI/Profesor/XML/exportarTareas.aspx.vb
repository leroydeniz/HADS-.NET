Imports System.Data.SqlClient
Imports System.Net
Imports System.Xml
Imports System.Xml.Serialization
Imports Newtonsoft.Json

Public Class exportarTareas

    Inherits System.Web.UI.Page
    Dim conexion As SqlConnection = New SqlConnection("Server=tcp:jorgehads.database.windows.net,1433;Initial Catalog=HADS-Jorge;Persist Security Info=False;User ID=trabajo.jorge2000@gmail.com@jorgehads;Password=Marmota69;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;")
    Dim dataAdapter As New SqlDataAdapter()
    Dim tareasTabla = New DataTable
    Dim tareasDataSet As New DataSet()
    Dim tareasFiltradasDataSet As New DataSet("tareas")
    Dim tareasFiltradas As New DataTable("tarea")
    Dim objController = New LAB.Controller




    Protected Sub VolverAlMenu_Click(sender As Object, e As EventArgs) Handles VolverAlMenu.Click
        Response.AddHeader("REFRESH", "0;URL=../inicioProfesor.aspx")
    End Sub




    Protected Sub DropDownList11_SelectedIndexChanged(sender As Object, e As EventArgs) Handles DropDownList11.SelectedIndexChanged

        If DropDownList11.Text = " " Then
            ExportarJSON.Enabled = False
            ExportarXML.Enabled = False
        End If

        Try
            ' Traigo la nueva asignatura elegida del DropDownList
            Session.Contents("asignaturaElegida") = DropDownList11.Text

            ' Cargo la tabla de todas las tareas que tengo en la sesión
            Dim tareasDataView As DataView = tareasTabla.DefaultView
            tareasDataView = New DataView(Session("tareasTabla"))

            ' Elijo el filtro que voy a hacer de toda la tabla completa
            Dim filtro As String = "CodAsig = '" & Session("asignaturaElegida") & "'"

            ' Aplico el filtro
            tareasDataView.RowFilter = filtro
            Session("tareasFiltradas") = tareasDataView.ToTable

            ' Pego la tabla filtrada en el DataView y aplico los cambios a tiempo real
            GridView1.DataSource = tareasDataView
            GridView1.DataBind()
        Catch ex As Exception
            Mensaje.Text = "DEBUG: DropDownList11_SelectedIndexChanged --> " + ex.Message
        End Try

    End Sub







    Protected Sub ExportarXML_Click(sender As Object, e As EventArgs) Handles ExportarXML.Click

        Try

            tareasFiltradas = Session("tareasFiltradas")
            tareasFiltradas.Columns.Item(0).ColumnMapping = MappingType.Attribute

            Dim tablaCopy As New DataTable("tarea")
            tablaCopy = tareasFiltradas.Copy()
            tablaCopy.TableName = "tarea"
            tareasFiltradasDataSet.Tables.Add(tablaCopy)

            ' Guardo el archivo
            tareasFiltradasDataSet.WriteXml(Server.MapPath("../../App_Data/" & Session("asignaturaElegida") & ".xml"))

            objController.registrarMovimiento(Session("usuario"), Session("tipo"), "Exportación XML")

            ' Descargo el archivo creado
            Response.ContentType = "application/octet-stream"
            Response.AppendHeader("Content-Disposition", "attachment; filename=" + Session("asignaturaElegida") + ".xml")
            Response.TransmitFile(Server.MapPath("../../App_Data/" + Session("asignaturaElegida") + ".xml"))
            Response.End()

        Catch ex As Exception
            Mensaje.Text = "DEBUG: ExportarXML_Click --> " + ex.Message
        End Try

    End Sub






    Protected Sub ExportarJSON_Click(sender As Object, e As EventArgs) Handles ExportarJSON.Click

        Dim tareasFiltradas As DataTable = Session("tareasFiltradas")

        Dim JSONString = String.Empty
        JSONString = JsonConvert.SerializeObject(tareasFiltradas)

        Dim objWriter As New System.IO.StreamWriter(Server.MapPath("../../App_Data/" + Session("asignaturaElegida") + ".json"))
        objWriter.Write(JSONString)
        objWriter.Close()

        objController.registrarMovimiento(Session("usuario"), Session("tipo"), "Exportación JSON")

        ' descargo el archivo creado
        Response.ContentType = "application/octet-stream"
        Response.AppendHeader("content-disposition", "attachment; filename=" + Session("asignaturaelegida") + ".json")
        Response.TransmitFile(Server.MapPath("../../App_Data/" + Session("asignaturaelegida") + ".json"))
        Response.End()

    End Sub






    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load

        ExportarJSON.Enabled = True
        ExportarXML.Enabled = True

        usuarioText.Text = Session.Contents("usuario")
            Session.Contents("asignaturaElegida") = DropDownList11.Text

        If Not Page.IsPostBack Then
            ExportarXML.Enabled = False
            ExportarJSON.Enabled = False
            Try
                Session.Contents("asignaturaElegida") = DropDownList11.Text

                ' 1 - SQL - Consulta de la tabla que trae
                Dim consulta As String = "SELECT Codigo, CodAsig, Descripcion, HEstimadas, Explotacion, TipoTarea FROM TareasGenericas;"

                ' 2 - Adapter - Ejecuta la consutla y establece la conexión
                dataAdapter = New SqlDataAdapter(consulta, conexion)

                ' 3 - SQLCommandBuilder - Establece automáticamente las consultas de AMB
                Dim tareasBuilder As New SqlCommandBuilder(dataAdapter)


                dataAdapter.Fill(tareasDataSet)

                tareasTabla = tareasDataSet.Tables(0)

                'GridView1.DataSource = tareasTabla
                'GridView1.DataBind()

                Session("dataAdapter") = dataAdapter
                Session("tareasDataSet") = tareasDataSet
                Session("tareasTabla") = tareasTabla

            Catch ex As Exception
                Mensaje.Text = "DEBUG: Page_Load --> " + ex.Message
            End Try
        End If
    End Sub






End Class
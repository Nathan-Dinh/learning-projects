# Create solution folder                                
mkdir ServiceMesh
cd ServiceMesh        
                                                
# Create solution                              
dotnet new sln -n ServiceMesh
                                            
# Create projects in src folder         
dotnet new webapi -n ServiceA -o src/csharp_PDFHandler
dotnet new webapi -n ServiceB -o src/csharp_PDF_Processing           
dotnet new webapi -n ServiceC -o src/csharp_Storage
dotnet new web -n Sidecar -o src/csharp_Sidecar                
dotnet new web -n ControlPlane -o src/csharp_ControlPlane
                                        
# Add projects to solution
dotnet sln add src/csharp_PDFHandler/csharp_PDFHandler.csproj             
dotnet sln add src/csharp_PDF_Processing/csharp_PDF_Processing.csproj
dotnet sln add src/csharp_Storage/csharp_Storage.csproj             
dotnet sln add src/csharp_Sidecar/csharp_Sidecar.csproj
dotnet sln add src/csharp_ControlPlane/csharp_ControlPlane.csproj
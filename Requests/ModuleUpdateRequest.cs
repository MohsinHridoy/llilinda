namespace Backend.Requests
{
    public class ModuleUpdateRequest
    {
        public string ModuleName { get; set; }
        public string Description { get; set; }
        public string ModulePackage { get; set; }
        public bool ModuleStatut { get; set; }

        public DateTime LastModificatedDate { get; set; }


        public List<int> ProductIds { get; set; }
        //public List<string> ProductNames { get;  set; }
    }
    
    
} 

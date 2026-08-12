// dat-skill-flow-build:20260811064050735-915b38a9cf8343ab8fc8d3b72a6909f7
                           
                  
                
 

                                
                              
                          
                            
                    
                        
                       
                       
                        
                            
                    

                                 
                             
                                  
                    
                    
                      
                  
                                                        
 

export function dataDiagnostic(
    code                    ,
    message        ,
    details                                                        = {},
)                 {
    return { code, severity: "error", message, ...details };
}

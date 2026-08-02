// dat-skill-flow-build:20260801124013015-2ff223378bdb460798e2143b569b741a
                           
                  
                
 

                                
                              
                          
                            
                    
                        
                       
                       
                        
                            
                    

                                 
                             
                                  
                    
                    
                      
                  
                                                        
 

export function dataDiagnostic(
    code                    ,
    message        ,
    details                                                        = {},
)                 {
    return { code, severity: "error", message, ...details };
}

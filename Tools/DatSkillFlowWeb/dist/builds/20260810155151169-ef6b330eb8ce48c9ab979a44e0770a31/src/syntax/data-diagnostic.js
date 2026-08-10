// dat-skill-flow-build:20260810155151169-ef6b330eb8ce48c9ab979a44e0770a31
                           
                  
                
 

                                
                              
                          
                            
                    
                        
                       
                       
                        
                            
                    

                                 
                             
                                  
                    
                    
                      
                  
                                                        
 

export function dataDiagnostic(
    code                    ,
    message        ,
    details                                                        = {},
)                 {
    return { code, severity: "error", message, ...details };
}

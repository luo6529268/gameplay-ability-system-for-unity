// dat-skill-flow-build:20260808082402712-d48a15db7dfc4da999c1921497d25414
                           
                  
                
 

                                
                              
                          
                            
                    
                        
                       
                       
                        
                            
                    

                                 
                             
                                  
                    
                    
                      
                  
                                                        
 

export function dataDiagnostic(
    code                    ,
    message        ,
    details                                                        = {},
)                 {
    return { code, severity: "error", message, ...details };
}

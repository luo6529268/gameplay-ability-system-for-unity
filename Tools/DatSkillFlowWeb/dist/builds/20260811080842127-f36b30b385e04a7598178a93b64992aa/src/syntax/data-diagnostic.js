// dat-skill-flow-build:20260811080842127-f36b30b385e04a7598178a93b64992aa
                           
                  
                
 

                                
                              
                          
                            
                    
                        
                       
                       
                        
                            
                    

                                 
                             
                                  
                    
                    
                      
                  
                                                        
 

export function dataDiagnostic(
    code                    ,
    message        ,
    details                                                        = {},
)                 {
    return { code, severity: "error", message, ...details };
}

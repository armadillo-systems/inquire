<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <xsl:output method="html" encoding="utf-8" indent="yes" />

 <xsl:template match="/">
   <xsl:text disable-output-escaping='yes'>&lt;!DOCTYPE html></xsl:text>
   <html>
     <body style='font-family: Arial'>
     <h2>RKD iNQUIRE Export</h2>
       <xsl:for-each select="items/item">
         <div>
           <img src="{Image}"/>
           <div>
             <strong>
               <u>
                 <xsl:value-of select="Title"/>
               </u>
             </strong>
           </div>
           <div>
             Author: <xsl:value-of select="Author"/>
           </div>
           <div>
             Collection: <xsl:value-of select="Collection"/>
           </div>
           <div>
             Date: <xsl:value-of select="Date"/>
           </div>
            <div>
             <xsl:value-of select="Attribution"/>
           </div>
           <div>
             Source: <xsl:value-of select="Category"/>
           </div>
           <div>
             Type: <xsl:value-of select="Keywords"/>
           </div>
         </div>
         <hr />
       </xsl:for-each>
   </body>
   </html>
 </xsl:template>

 </xsl:stylesheet>
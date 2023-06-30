<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
<xsl:template match="/book">
  <html>
  <body>
	<h2><xsl:value-of select="./title" /></h2>
	<h3><xsl:value-of select="./author" /></h3>
    <table border="1">
    <tr bgcolor="#9acd32">
      <th align="left">Name</th>
      <th align="left">Since</th>
	  <th align="left">Qualification</th>
    </tr>
    <xsl:for-each select="character">
    <tr>
      <td><xsl:value-of select="./name" /></td>
      <td><xsl:value-of select="./since" /></td>
	  <td><xsl:value-of select="./qualification" /></td>
    </tr>
    </xsl:for-each>
    </table>
  </body>
  </html>
</xsl:template>
</xsl:stylesheet>
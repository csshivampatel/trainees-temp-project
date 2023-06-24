<?xml version="1.0" encoding="utf-16"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:msxsl="urn:schemas-microsoft-com:xslt" version="1.0">
  <xsl:output omit-xml-declaration="yes" method="xml" version="1.0" />
  <xsl:template match="/">
    <xsl:apply-templates select="/Order" />
  </xsl:template>
  <xsl:template match="/Order">
    <Statement>
      <xsl:attribute name="type">
        <xsl:text>StoredProcedure</xsl:text>
      </xsl:attribute>
      <xsl:attribute name="sql">
        <xsl:text>{Call StoreOrderItem(?,?,?)}</xsl:text>
      </xsl:attribute>
      <Parameters>
        <Parameter>
          <xsl:attribute name="type">
            <xsl:text>int</xsl:text>
          </xsl:attribute>
          <xsl:attribute name="name">
            <xsl:text>@OrderID</xsl:text>
          </xsl:attribute>
          <xsl:attribute name="value">
            <xsl:value-of select="OrderID/text()" />
          </xsl:attribute>
        </Parameter>
        <Parameter>
          <xsl:attribute name="type">
            <xsl:text>datetime</xsl:text>
          </xsl:attribute>
          <xsl:attribute name="name">
            <xsl:text>@OrderDate</xsl:text>
          </xsl:attribute>
          <xsl:attribute name="value">
            <xsl:value-of select="OrderDate/text()" />
          </xsl:attribute>
        </Parameter>
        <Parameter>
          <xsl:attribute name="type">
            <xsl:text>money</xsl:text>
          </xsl:attribute>
          <xsl:attribute name="name">
            <xsl:text>@OrderAmount</xsl:text>
          </xsl:attribute>
          <xsl:attribute name="value">
            <xsl:value-of select="OrderAmount/text()" />
          </xsl:attribute>
        </Parameter>
      </Parameters>
    </Statement>
  </xsl:template>
</xsl:stylesheet>
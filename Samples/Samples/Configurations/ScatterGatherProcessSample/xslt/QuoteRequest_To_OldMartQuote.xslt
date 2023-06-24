<?xml version="1.0" encoding="UTF-16"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:msxsl="urn:schemas-microsoft-com:xslt" xmlns:var="http://schemas.microsoft.com/BizTalk/2003/var" exclude-result-prefixes="msxsl var" version="1.0">
  <xsl:output omit-xml-declaration="yes" method="xml" version="1.0" />
  <xsl:template match="/">
    <xsl:apply-templates select="/PurchaseRequest" />
  </xsl:template>
  <xsl:template match="/PurchaseRequest">
      <QuoteRequest xmlns="http://schema.neuron.sample/oldmart/broadcast/request">
        <Products>
          <Product>
            <xsl:attribute name="SKU">
              <xsl:value-of select="Products/Product/@name" />
            </xsl:attribute>
            <xsl:attribute name="quanity">
              <xsl:value-of select="Products/Product/@quanity" />
            </xsl:attribute>
            <xsl:attribute name="warehouse">
              <xsl:value-of select="Products/Product/@location" />
            </xsl:attribute>
          </Product>
        </Products>
      </QuoteRequest>
  </xsl:template>
</xsl:stylesheet>
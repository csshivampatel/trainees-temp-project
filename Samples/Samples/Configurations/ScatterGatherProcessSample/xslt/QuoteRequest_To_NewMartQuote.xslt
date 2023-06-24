<?xml version="1.0" encoding="UTF-16"?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:msxsl="urn:schemas-microsoft-com:xslt" xmlns:var="http://schemas.microsoft.com/BizTalk/2003/var" exclude-result-prefixes="msxsl var" version="1.0">
  <xsl:output omit-xml-declaration="yes" method="xml" version="1.0" />
  <xsl:template match="/">
    <xsl:apply-templates select="/PurchaseRequest" />
  </xsl:template>
  <xsl:template match="/PurchaseRequest">
      <BidQuery xmlns="http://schema.neuron.sample/newmart/broadcast/request">
        <Catalog>
          <xsl:attribute name="productId">
            <xsl:value-of select="Products/Product/@name" />
          </xsl:attribute>
          <xsl:attribute name="qty">
            <xsl:value-of select="Products/Product/@quanity" />
          </xsl:attribute>
          <xsl:attribute name="distributionCenter">
            <xsl:value-of select="Products/Product/@location" />
          </xsl:attribute>
        </Catalog>
      </BidQuery>
  </xsl:template>
</xsl:stylesheet>
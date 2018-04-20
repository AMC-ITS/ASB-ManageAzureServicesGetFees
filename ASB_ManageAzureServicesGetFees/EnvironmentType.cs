namespace ASB_ManageAzureServicesGetFees
{
    public enum EnvironmentType
    {
        /// <summary>
        /// Undefined
        /// </summary>
        Unknown,
        /// <summary>
        /// DEV.  Used by the software engineers as a sandbox for creating and testing bugs and/or features.
        /// </summary>
        Development,
        /// <summary>
        /// Used by QA, LOS Admins, and business for initial testing and approval of fixes and/or features.
        /// </summary>
        UserAcceptanceTesting,
        /// <summary>
        /// A replica of production intended for integration and regression testing.
        /// </summary>
        Staging,
        /// <summary>
        /// The real live customer facing product.
        /// </summary>
        Production
    }
}

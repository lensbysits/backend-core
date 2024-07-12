namespace Lens.Core.Lib.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class SwaggerAdditionalModelAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of <see cref="SwaggerAdditionalModelAttribute"/>
        /// </summary>
        /// <param name="groupNames">Contains the names of the swagger groups to which the model should be added.</param>
        public SwaggerAdditionalModelAttribute(params string[] groupNames)
        {
            this.GroupNames = groupNames.ToList() ?? new();
        }

        public List<string> GroupNames { get; }
    }
}

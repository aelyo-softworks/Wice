namespace DirectN
{
    public partial struct D2D1_BEZIER_SEGMENT
    {
        public D2D1_BEZIER_SEGMENT(D2D_POINT_2F point1, D2D_POINT_2F point2, D2D_POINT_2F point3)
        {
            this.point1 = point1;
            this.point2 = point2;
            this.point3 = point3;
        }
    }
}

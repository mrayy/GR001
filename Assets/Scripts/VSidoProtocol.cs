using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

    struct SERVO_PROP
    {
        public short rom_model_num;     /*!< サーボモデル番号*/
        public byte rom_servo_ID;      /*!< ID*/
        public short rom_cw_agl_lmt;    /*!< 時計回り回転リミット角*/
        public short rom_ccw_agl_lmt;   /*!< 反時計回り回転リミット角*/
        public sbyte rom_damper;        /*!< ダンパー*/
        public sbyte rom_cw_cmp_margin; /*!< コンプライアンスマージン*/
        public sbyte rom_ccw_cmp_margin;/*!< コンプライアンスマージン*/
        public sbyte rom_cw_cmp_slope;  /*!< コンプライアンススロープ*/
        public sbyte rom_ccw_cmp_slope; /*!< コンプライアンススロープ*/
        public sbyte rom_punch;         /*!< パンチ*/
        public short ram_goal_pos;      /*!< 目標角度*/
        public short ram_goal_tim;      /*!< 速度（目標到達時間を10ms刻みで指定）*/
        public byte rom_max_torque;    /*!< 最大トルク*/
        public byte ram_torque_mode;   /*!< トルクモード（0:off,1:on,2:break）*/
        public short ram_pres_pos;      /*!< 現在角度*/
        public short ram_pres_time;     /*!< 現在時間（移動コマンド受信時から10ms刻みで指定）*/
        public short ram_pres_spd;      /*!< 現在速度（参考程度）*/
        public short ram_pres_curr;     /*!< 現在電流*/
        public short ram_pres_temp;     /*!< 現在温度*/
        public short ram_pres_volt;     /*!< 現在電圧*/
        public byte flags;             /*!< サーボのリターンフラグ（温度エラーなど）*/
        public short agl_ofset;         /*!< トリム角(現在角はram_pres_pos-agl_ofset)*/
        public byte parents_ID;        /*!< ダブルサーボ時のID*/
        public byte connected;         /*!< サーボ接続の有無（０，１）*/
        public short read_time;         /*!< 関節角度の受信にかかった時間*/
        public short _ram_goal_pos;     /*!< 前回の目標角度*/
        public short __ram_goal_pos;    /*!< 前前回の目標角度*/
        public short _ram_pres_pos;     /*!< 前回の現在角度*/
        public sbyte _send_speed;       /*!< 前回の目標速度*/
        public byte _send_cmd_time;    /*!< 前回のlong packetコマンド送信時間(10ms) */
        public byte flg_min_max;       /*!< 現在角＞最大角のとき１，現在角＜最小角のとき?１，通常０*/
        public byte flg_goal_pos;      /*!< 目標角度に変化があったとき1，変化がないとき0*/
        public byte flg_parent_inv;    /*!< ダブルサーボ時の逆転*/
        public byte flg_cmp_slope;     /*!< コンプライアンススロープに変化があったとき1，変化がないとき0*/
        public byte flg_check_angle;   /*!< 常に角度情報を監視するか否か*/
        public sbyte port_type;           /*!< ttl接続なのかrs485接続か*/
        public sbyte servo_type;         /*!< サーボメーカー*/
    };

    enum CMD_TYPE
    {
        set_Object,
        set_Conpriance,
        set_Min_max,
        set_Feedback_ID,
        get_Read_feedback_data,
        get_Data_of_servo,
        set_Set_flash_data,
        get_Get_flash_data,
        set_Write_flash_data,
        set_IO,
        set_PWM,
        get_Joint_state,
        set_get_IK,
        set_walk_To,
        get_Acc_data,
        get_Voltage_data
    };

	class VSidoProtocol
    {
        private byte[] gen_cmd( byte op, byte[] data)
        {
            int data_len = data.Length;
            byte[] cmd_buf = new byte[4+data_len];
            int sl=0;
            byte sum = 0;

            cmd_buf[sl++] = 0xff;
            cmd_buf[sl++] = op;
            cmd_buf[sl++] = (byte)(4 + data_len);
            for (int i = 0; i < data_len; i++)
            {
                cmd_buf[sl++] = data[i];
            }

            for (int i = 0; i < sl; i++)
                sum = (byte)(sum ^ cmd_buf[i]);//check sum
            cmd_buf[sl] = sum;
            return cmd_buf;
        }

        private byte[] conv_angle(Int16 angle)
        {
            Int16 shifted_angle = (Int16)(angle << 1);
            byte[] byteArray = BitConverter.GetBytes(shifted_angle);//byte型に分割

            byte data_low = byteArray[0];
            byte data_high = byteArray[1];

            byte[] r_data= {0,0};
            r_data[0]= data_low;
            r_data[1] = (byte)(data_high << 1);

            return r_data;
        }

        public byte[] gen_cmd_set_object(byte id, short angle)
        {
            byte[] data_bytes = new byte[4];
            byte[] converted_angle = conv_angle(angle);
//            byte[] converted_angle = conv_angle(900);

           data_bytes[0] =2;//cycle
           data_bytes[1] =id;//id
           data_bytes[2] = converted_angle[0];//angle_data_low
           data_bytes[3] = converted_angle[1];//angle_data_high

            //必要時応じて繰り返し


            byte[] send_buf = gen_cmd((byte)'o',data_bytes);//opは'o'
           return send_buf;
	}

	public struct JointAnglePair
	{
		public byte id;
		public short angle;
	}
	public byte[] gen_cmd_set_objects(JointAnglePair[] pairs)
	{
		if (pairs.Length == 0)
						return null;
		byte[] data_bytes = new byte[1+3*pairs.Length];
		//            byte[] converted_angle = conv_angle(900);
		
		data_bytes[0] =2;//cycle
		for(int i=0;i<pairs.Length;i++)
		{
			byte[] converted_angle = conv_angle(pairs[i].angle);
			data_bytes[3*i+1] =pairs[i].id;//id
			data_bytes[3*i+2] = converted_angle[0];//angle_data_low
			data_bytes[3*i+3] = converted_angle[1];//angle_data_high
		}
		//必要時応じて繰り返し
		
		
		byte[] send_buf = gen_cmd((byte)'o',data_bytes);//opは'o'
		return send_buf;
	}

	//Request the value of certain joint
	public byte[] gen_cmd_get_object(byte id)
	{
		byte[] data_bytes = new byte[4];
		
		data_bytes[0] =id;//id
		
		//必要時応じて繰り返し
		
		
		byte[] send_buf = gen_cmd((byte)'o',data_bytes);//opは'o'
		return send_buf;
	}

        public byte[] gen_cmd_get_IK(byte id)
        {
            byte[] data_bytes = new byte[2];
            byte ikf = 56;//現在値要求
            byte kid = id;

            data_bytes[0] = ikf;

            //必要時応じて繰り返し
            data_bytes[1] = kid;
            byte[] send_buf = gen_cmd((byte)'k', data_bytes);
            return send_buf;
        }

        public byte[] gen_cmd_set_IK(byte id, int ik_x, int ik_y, int ik_z)
        {
            byte[] data_bytes = new byte[11];
            byte ikf = 7;//目標値設定
            byte kid = id;
            data_bytes[0] = ikf;

            /*
             * ik_data[L_HAND].x= 50+20*sin(cnt*M_PI/180);//left
             * ik_data[L_HAND].y=-60;//back
             * ik_data[L_HAND].z= 30;//up
             * ik_data[L_HAND].flg_ik=1; 
             * ik_data[R_HAND].x= -50-20*sin(cnt*M_PI/180);//left    
             * ik_data[R_HAND].y=-60;//back    
             * ik_data[R_HAND].z= 30;//up    
             * ik_data[R_HAND].flg_ik=1;
            */
            data_bytes[1] = kid;
            data_bytes[2] = Convert.ToByte(ik_x +100);
            data_bytes[3] = Convert.ToByte(ik_y + 100);
            data_bytes[4] = Convert.ToByte(ik_z + 100);
            data_bytes[5] = 0;
            data_bytes[6] = 0;
            data_bytes[7] = 0;
            data_bytes[8] = 0;
            data_bytes[9] = 0;
            data_bytes[10] = 0;

            byte[] send_buf = gen_cmd((byte)'k', data_bytes);
            return send_buf;
        }

        public byte[] gen_cmd_set_walk(int wspeed,int wturn, int wtempo)
        {
            byte[] data_bytes = new byte[5];

            byte address=0;//最初の文字
            byte data_len=3;//データ数

            data_bytes[0]=address;
            data_bytes[1]=data_len;
            data_bytes[2] = Convert.ToByte(wspeed + 100);
            data_bytes[3] = Convert.ToByte(wturn + 100);
            data_bytes[4] = Convert.ToByte(wtempo + 100);
            
            byte[] send_buf = gen_cmd((byte)'t', data_bytes);
            return send_buf;
        }

}



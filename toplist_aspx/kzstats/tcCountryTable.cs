using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Web.UI.WebControls;
using System.Drawing;
using System.Data;
using System.Diagnostics;
using System.Web;
using System.Web.UI;
using System.Collections;

namespace classes
{
    //Instead of requing all sorts of SQL to return full country names, use this look-up table with the country code as key
    public class tcCountryTable
    {
        private static Dictionary<string,string> mcCountryTable = null;

        public static string CodeToFull(string apanCode)
        {
            string lpanFullName = null;
            
            if (mcCountryTable == null)
            {
                mcCountryTable = InitializeTable();
            }

            try
            {
                lpanFullName = mcCountryTable[apanCode];
            }
            catch
            {
                //Do nothing
            }
            finally
            {
                if (lpanFullName == null)
                {
                    lpanFullName = "";
                }
            }

            return lpanFullName;
        }

        private static Dictionary<string,string> InitializeTable()
        {
            //Note capacity is set roughly 2x the number of entries, this reduces collision and increases look-up speed
            Dictionary<string, string> lcReturn = new Dictionary<string, string>(500);

            lcReturn.Add("A1", "Anonymous Proxy");
            lcReturn.Add("A2", "Satellite Provider");
            lcReturn.Add("AD", "Andorra");
            lcReturn.Add("AE", "United Arab Emirates");
            lcReturn.Add("AF", "Afghanistan");
            lcReturn.Add("AG", "Antigua and Barbuda");
            lcReturn.Add("AI", "Anguilla");
            lcReturn.Add("AL", "Albania");
            lcReturn.Add("AM", "Armenia");
            lcReturn.Add("AN", "Netherlands Antilles");
            lcReturn.Add("AO", "Angola");
            lcReturn.Add("AP", "Asia/Pacific Region");
            lcReturn.Add("AQ", "Antarctica");
            lcReturn.Add("AR", "Argentina");
            lcReturn.Add("AS", "American Samoa");
            lcReturn.Add("AT", "Austria");
            lcReturn.Add("AU", "Australia");
            lcReturn.Add("AW", "Aruba");
            lcReturn.Add("AX", "Aland Islands");
            lcReturn.Add("AZ", "Azerbaijan");
            lcReturn.Add("BA", "Bosnia and Herzegovina");
            lcReturn.Add("BB", "Barbados");
            lcReturn.Add("BD", "Bangladesh");
            lcReturn.Add("BE", "Belgium");
            lcReturn.Add("BF", "Burkina Faso");
            lcReturn.Add("BG", "Bulgaria");
            lcReturn.Add("BH", "Bahrain");
            lcReturn.Add("BI", "Burundi");
            lcReturn.Add("BJ", "Benin");
            lcReturn.Add("BM", "Bermuda");
            lcReturn.Add("BN", "Brunei Darussalam");
            lcReturn.Add("BO", "Bolivia");
            lcReturn.Add("BR", "Brazil");
            lcReturn.Add("BS", "Bahamas");
            lcReturn.Add("BT", "Bhutan");
            lcReturn.Add("BV", "Bouvet Island");
            lcReturn.Add("BW", "Botswana");
            lcReturn.Add("BY", "Belarus");
            lcReturn.Add("BZ", "Belize");
            lcReturn.Add("CA", "Canada");
            lcReturn.Add("CC", "Cocos (Keeling) Islands");
            lcReturn.Add("CD", "Congo, The Democratic Republic of the");
            lcReturn.Add("CF", "Central African Republic");
            lcReturn.Add("CG", "Congo");
            lcReturn.Add("CH", "Switzerland");
            lcReturn.Add("CI", "Cote D'Ivoire");
            lcReturn.Add("CK", "Cook Islands");
            lcReturn.Add("CL", "Chile");
            lcReturn.Add("CM", "Cameroon");
            lcReturn.Add("CN", "China");
            lcReturn.Add("CO", "Colombia");
            lcReturn.Add("CR", "Costa Rica");
            lcReturn.Add("CU", "Cuba");
            lcReturn.Add("CV", "Cape Verde");
            lcReturn.Add("CX", "Christmas Island");
            lcReturn.Add("CY", "Cyprus");
            lcReturn.Add("CZ", "Czech Republic");
            lcReturn.Add("DE", "Germany");
            lcReturn.Add("DJ", "Djibouti");
            lcReturn.Add("DK", "Denmark");
            lcReturn.Add("DM", "Dominica");
            lcReturn.Add("DO", "Dominican Republic");
            lcReturn.Add("DZ", "Algeria");
            lcReturn.Add("EC", "Ecuador");
            lcReturn.Add("EE", "Estonia");
            lcReturn.Add("EG", "Egypt");
            lcReturn.Add("EH", "Western Sahara");
            lcReturn.Add("ER", "Eritrea");
            lcReturn.Add("ES", "Spain");
            lcReturn.Add("ET", "Ethiopia");
            lcReturn.Add("EU", "Europe");
            lcReturn.Add("FI", "Finland");
            lcReturn.Add("FJ", "Fiji");
            lcReturn.Add("FK", "Falkland Islands (Malvinas)");
            lcReturn.Add("FM", "Micronesia, Federated States of");
            lcReturn.Add("FO", "Faroe Islands");
            lcReturn.Add("FR", "France");
            lcReturn.Add("GA", "Gabon");
            lcReturn.Add("GB", "United Kingdom");
            lcReturn.Add("GD", "Grenada");
            lcReturn.Add("GE", "Georgia");
            lcReturn.Add("GF", "French Guiana");
            lcReturn.Add("GG", "Guernsey");
            lcReturn.Add("GH", "Ghana");
            lcReturn.Add("GI", "Gibraltar");
            lcReturn.Add("GL", "Greenland");
            lcReturn.Add("GM", "Gambia");
            lcReturn.Add("GN", "Guinea");
            lcReturn.Add("GP", "Guadeloupe");
            lcReturn.Add("GQ", "Equatorial Guinea");
            lcReturn.Add("GR", "Greece");
            lcReturn.Add("GS", "South Georgia and the South Sandwich Islands");
            lcReturn.Add("GT", "Guatemala");
            lcReturn.Add("GU", "Guam");
            lcReturn.Add("GW", "Guinea-Bissau");
            lcReturn.Add("GY", "Guyana");
            lcReturn.Add("HK", "Hong Kong");
            lcReturn.Add("HN", "Honduras");
            lcReturn.Add("HR", "Croatia");
            lcReturn.Add("HT", "Haiti");
            lcReturn.Add("HU", "Hungary");
            lcReturn.Add("ID", "Indonesia");
            lcReturn.Add("IE", "Ireland");
            lcReturn.Add("IL", "Israel");
            lcReturn.Add("IM", "Isle of Man");
            lcReturn.Add("IN", "India");
            lcReturn.Add("IO", "British Indian Ocean Territory");
            lcReturn.Add("IQ", "Iraq");
            lcReturn.Add("IR", "Iran, Islamic Republic of");
            lcReturn.Add("IS", "Iceland");
            lcReturn.Add("IT", "Italy");
            lcReturn.Add("JE", "Jersey");
            lcReturn.Add("JM", "Jamaica");
            lcReturn.Add("JO", "Jordan");
            lcReturn.Add("JP", "Japan");
            lcReturn.Add("KE", "Kenya");
            lcReturn.Add("KG", "Kyrgyzstan");
            lcReturn.Add("KH", "Cambodia");
            lcReturn.Add("KI", "Kiribati");
            lcReturn.Add("KM", "Comoros");
            lcReturn.Add("KN", "Saint Kitts and Nevis");
            lcReturn.Add("KP", "Korea, Democratic People's Republic of");
            lcReturn.Add("KR", "Korea, Republic of");
            lcReturn.Add("KW", "Kuwait");
            lcReturn.Add("KY", "Cayman Islands");
            lcReturn.Add("KZ", "Kazakhstan");
            lcReturn.Add("LA", "Lao People's Democratic Republic");
            lcReturn.Add("LB", "Lebanon");
            lcReturn.Add("LC", "Saint Lucia");
            lcReturn.Add("LI", "Liechtenstein");
            lcReturn.Add("LK", "Sri Lanka");
            lcReturn.Add("LR", "Liberia");
            lcReturn.Add("LS", "Lesotho");
            lcReturn.Add("LT", "Lithuania");
            lcReturn.Add("LU", "Luxembourg");
            lcReturn.Add("LV", "Latvia");
            lcReturn.Add("LY", "Libyan Arab Jamahiriya");
            lcReturn.Add("MA", "Morocco");
            lcReturn.Add("MC", "Monaco");
            lcReturn.Add("MD", "Moldova, Republic of");
            lcReturn.Add("ME", "Montenegro");
            lcReturn.Add("MG", "Madagascar");
            lcReturn.Add("MH", "Marshall Islands");
            lcReturn.Add("MK", "Macedonia");
            lcReturn.Add("ML", "Mali");
            lcReturn.Add("MM", "Myanmar");
            lcReturn.Add("MN", "Mongolia");
            lcReturn.Add("MO", "Macau");
            lcReturn.Add("MP", "Northern Mariana Islands");
            lcReturn.Add("MQ", "Martinique");
            lcReturn.Add("MR", "Mauritania");
            lcReturn.Add("MS", "Montserrat");
            lcReturn.Add("MT", "Malta");
            lcReturn.Add("MU", "Mauritius");
            lcReturn.Add("MV", "Maldives");
            lcReturn.Add("MW", "Malawi");
            lcReturn.Add("MX", "Mexico");
            lcReturn.Add("MY", "Malaysia");
            lcReturn.Add("MZ", "Mozambique");
            lcReturn.Add("NA", "Namibia");
            lcReturn.Add("NC", "New Caledonia");
            lcReturn.Add("NE", "Niger");
            lcReturn.Add("NF", "Norfolk Island");
            lcReturn.Add("NG", "Nigeria");
            lcReturn.Add("NI", "Nicaragua");
            lcReturn.Add("NL", "Netherlands");
            lcReturn.Add("NO", "Norway");
            lcReturn.Add("NP", "Nepal");
            lcReturn.Add("NR", "Nauru");
            lcReturn.Add("NU", "Niue");
            lcReturn.Add("NZ", "New Zealand");
            lcReturn.Add("OM", "Oman");
            lcReturn.Add("PA", "Panama");
            lcReturn.Add("PE", "Peru");
            lcReturn.Add("PF", "French Polynesia");
            lcReturn.Add("PG", "Papua New Guinea");
            lcReturn.Add("PH", "Philippines");
            lcReturn.Add("PK", "Pakistan");
            lcReturn.Add("PL", "Poland");
            lcReturn.Add("PM", "Saint Pierre and Miquelon");
            lcReturn.Add("PR", "Puerto Rico");
            lcReturn.Add("PS", "Palestinian Territory, Occupied");
            lcReturn.Add("PT", "Portugal");
            lcReturn.Add("PW", "Palau");
            lcReturn.Add("PY", "Paraguay");
            lcReturn.Add("QA", "Qatar");
            lcReturn.Add("RE", "Reunion");
            lcReturn.Add("RO", "Romania");
            lcReturn.Add("RS", "Serbia");
            lcReturn.Add("RU", "Russian Federation");
            lcReturn.Add("RW", "Rwanda");
            lcReturn.Add("SA", "Saudi Arabia");
            lcReturn.Add("SB", "Solomon Islands");
            lcReturn.Add("SC", "Seychelles");
            lcReturn.Add("SD", "Sudan");
            lcReturn.Add("SE", "Sweden");
            lcReturn.Add("SG", "Singapore");
            lcReturn.Add("SH", "Saint Helena");
            lcReturn.Add("SI", "Slovenia");
            lcReturn.Add("SJ", "Svalbard and Jan Mayen");
            lcReturn.Add("SK", "Slovakia");
            lcReturn.Add("SL", "Sierra Leone");
            lcReturn.Add("SM", "San Marino");
            lcReturn.Add("SN", "Senegal");
            lcReturn.Add("SO", "Somalia");
            lcReturn.Add("SR", "Suriname");
            lcReturn.Add("ST", "Sao Tome and Principe");
            lcReturn.Add("SV", "El Salvador");
            lcReturn.Add("SY", "Syrian Arab Republic");
            lcReturn.Add("SZ", "Swaziland");
            lcReturn.Add("TC", "Turks and Caicos Islands");
            lcReturn.Add("TD", "Chad");
            lcReturn.Add("TF", "French Southern Territories");
            lcReturn.Add("TG", "Togo");
            lcReturn.Add("TH", "Thailand");
            lcReturn.Add("TJ", "Tajikistan");
            lcReturn.Add("TK", "Tokelau");
            lcReturn.Add("TL", "Timor-Leste");
            lcReturn.Add("TM", "Turkmenistan");
            lcReturn.Add("TN", "Tunisia");
            lcReturn.Add("TO", "Tonga");
            lcReturn.Add("TR", "Turkey");
            lcReturn.Add("TT", "Trinidad and Tobago");
            lcReturn.Add("TV", "Tuvalu");
            lcReturn.Add("TW", "Taiwan");
            lcReturn.Add("TZ", "Tanzania, United Republic of");
            lcReturn.Add("UA", "Ukraine");
            lcReturn.Add("UG", "Uganda");
            lcReturn.Add("UM", "United States Minor Outlying Islands");
            lcReturn.Add("US", "United States");
            lcReturn.Add("UY", "Uruguay");
            lcReturn.Add("UZ", "Uzbekistan");
            lcReturn.Add("VA", "Holy See (Vatican City State)");
            lcReturn.Add("VC", "Saint Vincent and the Grenadines");
            lcReturn.Add("VE", "Venezuela");
            lcReturn.Add("VG", "Virgin Islands, British");
            lcReturn.Add("VI", "Virgin Islands, U.S.");
            lcReturn.Add("VN", "Vietnam");
            lcReturn.Add("VU", "Vanuatu");
            lcReturn.Add("WF", "Wallis and Futuna");
            lcReturn.Add("WS", "Samoa");
            lcReturn.Add("YE", "Yemen");
            lcReturn.Add("YT", "Mayotte");
            lcReturn.Add("ZA", "South Africa");
            lcReturn.Add("ZM", "Zambia");
            lcReturn.Add("ZW", "Zimbabwe");

            return lcReturn;
        }

    }
}

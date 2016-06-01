using NSubstitute;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wyam.Common.Documents;
using Wyam.Common.Execution;
using Wyam.Testing;

namespace Wyam.Tables.Tests
{
    [TestFixture]
    public class CsvToHtmlTests : BaseFixture
    {
        public class ExecuteMethodTests : CsvToHtmlTests
        {
            [Test]
            public void TestWithoutHeadder()
            {
                // Given
                #region input
                string input = ""
        + "\"\",\"A\",\"B\",\"C\",\"D\",\"E\",\"F\",\"G\"\r\n"
+ "\"1\",\"1\",\"2\",\"3\",\"4\",\"5\",\"6\",\"7\"\r\n"
+ "\"2\",\"2\",\"4\",\"6\",\"8\",\"10\",\"12\",\"14\"\r\n"
+ "\"3\",\"3\",\"6\",\"9\",\"12\",\"15\",\"18\",\"21\"\r\n"
+ "\"4\",\"4\",\"8\",\"12\",\"16\",\"20\",\"24\",\"28\"\r\n"
+ "\"5\",\"5\",\"10\",\"15\",\"20\",\"25\",\"30\",\"35\"\r\n"
+ "\"6\",\"6\",\"12\",\"18\",\"24\",\"30\",\"36\",\"42\"\r\n"
+ "\"7\",\"7\",\"14\",\"21\",\"28\",\"35\",\"42\",\"49\"\r\n"
+ "\"8\",\"8\",\"16\",\"24\",\"32\",\"40\",\"48\",\"56\"\r\n"
+ "\"9\",\"9\",\"18\",\"27\",\"36\",\"45\",\"54\",\"63\"\r\n"
+ "\"10\",\"10\",\"20\",\"30\",\"40\",\"50\",\"60\",\"70\"\r\n"
+ "\"11\",\"11\",\"22\",\"33\",\"44\",\"55\",\"66\",\"77\"\r\n"
+ "\"12\",\"12\",\"24\",\"36\",\"48\",\"60\",\"72\",\"84\"\r\n"
+ "\"13\",\"13\",\"26\",\"39\",\"52\",\"65\",\"78\",\"91\"\r\n"
+ "\"14\",\"14\",\"28\",\"42\",\"56\",\"70\",\"84\",\"98\"\r\n"
+ "\"15\",\"15\",\"30\",\"45\",\"60\",\"75\",\"90\",\"105\"\r\n"
+ "\"16\",\"16\",\"32\",\"48\",\"64\",\"80\",\"96\",\"112\"\r\n"
+ "\"17\",\"17\",\"34\",\"51\",\"68\",\"85\",\"102\",\"119\"\r\n"
+ "\"18\",\"18\",\"36\",\"54\",\"72\",\"90\",\"108\",\"126\"\r\n"
+ "\"19\",\"19\",\"38\",\"57\",\"76\",\"95\",\"114\",\"133\"\r\n"
+ "\"20\",\"20\",\"40\",\"60\",\"80\",\"100\",\"120\",\"140\"\r\n"
+ "\"21\",\"21\",\"42\",\"63\",\"84\",\"105\",\"126\",\"147\"\r\n"
+ "\"22\",\"22\",\"44\",\"66\",\"88\",\"110\",\"132\",\"154\"\r\n"
+ "\"23\",\"23\",\"46\",\"69\",\"92\",\"115\",\"138\",\"161\"\r\n"
+ "\"24\",\"24\",\"48\",\"72\",\"96\",\"120\",\"144\",\"168\"\r\n"
+ "\"25\",\"25\",\"50\",\"75\",\"100\",\"125\",\"150\",\"175\"\r\n"
+ "\"26\",\"26\",\"52\",\"78\",\"104\",\"130\",\"156\",\"182\"\r\n";

                string output = @"<table>
<tr>
<td></td>
<td>A</td>
<td>B</td>
<td>C</td>
<td>D</td>
<td>E</td>
<td>F</td>
<td>G</td>
</tr>
<tr>
<td>1</td>
<td>1</td>
<td>2</td>
<td>3</td>
<td>4</td>
<td>5</td>
<td>6</td>
<td>7</td>
</tr>
<tr>
<td>2</td>
<td>2</td>
<td>4</td>
<td>6</td>
<td>8</td>
<td>10</td>
<td>12</td>
<td>14</td>
</tr>
<tr>
<td>3</td>
<td>3</td>
<td>6</td>
<td>9</td>
<td>12</td>
<td>15</td>
<td>18</td>
<td>21</td>
</tr>
<tr>
<td>4</td>
<td>4</td>
<td>8</td>
<td>12</td>
<td>16</td>
<td>20</td>
<td>24</td>
<td>28</td>
</tr>
<tr>
<td>5</td>
<td>5</td>
<td>10</td>
<td>15</td>
<td>20</td>
<td>25</td>
<td>30</td>
<td>35</td>
</tr>
<tr>
<td>6</td>
<td>6</td>
<td>12</td>
<td>18</td>
<td>24</td>
<td>30</td>
<td>36</td>
<td>42</td>
</tr>
<tr>
<td>7</td>
<td>7</td>
<td>14</td>
<td>21</td>
<td>28</td>
<td>35</td>
<td>42</td>
<td>49</td>
</tr>
<tr>
<td>8</td>
<td>8</td>
<td>16</td>
<td>24</td>
<td>32</td>
<td>40</td>
<td>48</td>
<td>56</td>
</tr>
<tr>
<td>9</td>
<td>9</td>
<td>18</td>
<td>27</td>
<td>36</td>
<td>45</td>
<td>54</td>
<td>63</td>
</tr>
<tr>
<td>10</td>
<td>10</td>
<td>20</td>
<td>30</td>
<td>40</td>
<td>50</td>
<td>60</td>
<td>70</td>
</tr>
<tr>
<td>11</td>
<td>11</td>
<td>22</td>
<td>33</td>
<td>44</td>
<td>55</td>
<td>66</td>
<td>77</td>
</tr>
<tr>
<td>12</td>
<td>12</td>
<td>24</td>
<td>36</td>
<td>48</td>
<td>60</td>
<td>72</td>
<td>84</td>
</tr>
<tr>
<td>13</td>
<td>13</td>
<td>26</td>
<td>39</td>
<td>52</td>
<td>65</td>
<td>78</td>
<td>91</td>
</tr>
<tr>
<td>14</td>
<td>14</td>
<td>28</td>
<td>42</td>
<td>56</td>
<td>70</td>
<td>84</td>
<td>98</td>
</tr>
<tr>
<td>15</td>
<td>15</td>
<td>30</td>
<td>45</td>
<td>60</td>
<td>75</td>
<td>90</td>
<td>105</td>
</tr>
<tr>
<td>16</td>
<td>16</td>
<td>32</td>
<td>48</td>
<td>64</td>
<td>80</td>
<td>96</td>
<td>112</td>
</tr>
<tr>
<td>17</td>
<td>17</td>
<td>34</td>
<td>51</td>
<td>68</td>
<td>85</td>
<td>102</td>
<td>119</td>
</tr>
<tr>
<td>18</td>
<td>18</td>
<td>36</td>
<td>54</td>
<td>72</td>
<td>90</td>
<td>108</td>
<td>126</td>
</tr>
<tr>
<td>19</td>
<td>19</td>
<td>38</td>
<td>57</td>
<td>76</td>
<td>95</td>
<td>114</td>
<td>133</td>
</tr>
<tr>
<td>20</td>
<td>20</td>
<td>40</td>
<td>60</td>
<td>80</td>
<td>100</td>
<td>120</td>
<td>140</td>
</tr>
<tr>
<td>21</td>
<td>21</td>
<td>42</td>
<td>63</td>
<td>84</td>
<td>105</td>
<td>126</td>
<td>147</td>
</tr>
<tr>
<td>22</td>
<td>22</td>
<td>44</td>
<td>66</td>
<td>88</td>
<td>110</td>
<td>132</td>
<td>154</td>
</tr>
<tr>
<td>23</td>
<td>23</td>
<td>46</td>
<td>69</td>
<td>92</td>
<td>115</td>
<td>138</td>
<td>161</td>
</tr>
<tr>
<td>24</td>
<td>24</td>
<td>48</td>
<td>72</td>
<td>96</td>
<td>120</td>
<td>144</td>
<td>168</td>
</tr>
<tr>
<td>25</td>
<td>25</td>
<td>50</td>
<td>75</td>
<td>100</td>
<td>125</td>
<td>150</td>
<td>175</td>
</tr>
<tr>
<td>26</td>
<td>26</td>
<td>52</td>
<td>78</td>
<td>104</td>
<td>130</td>
<td>156</td>
<td>182</td>
</tr>
</table>"; 
                #endregion
                IDocument document = Substitute.For<IDocument>();

                MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(input));
                document.GetStream().Returns(stream);
                document.Content.Returns(input);
                IExecutionContext context = Substitute.For<IExecutionContext>();

                CsvToHtml module = new CsvToHtml();

                // When
                module.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

                // Then
                context.Received().GetDocument(document, output);
                stream.Dispose();
            }

            [Test]
            public void TestHeadder()
            {
                // Given
                #region input
                string input = ""
        + "\"\",\"A\",\"B\",\"C\",\"D\",\"E\",\"F\",\"G\"\r\n"
+ "\"1\",\"1\",\"2\",\"3\",\"4\",\"5\",\"6\",\"7\"\r\n"
+ "\"2\",\"2\",\"4\",\"6\",\"8\",\"10\",\"12\",\"14\"\r\n"
+ "\"3\",\"3\",\"6\",\"9\",\"12\",\"15\",\"18\",\"21\"\r\n"
+ "\"4\",\"4\",\"8\",\"12\",\"16\",\"20\",\"24\",\"28\"\r\n"
+ "\"5\",\"5\",\"10\",\"15\",\"20\",\"25\",\"30\",\"35\"\r\n"
+ "\"6\",\"6\",\"12\",\"18\",\"24\",\"30\",\"36\",\"42\"\r\n"
+ "\"7\",\"7\",\"14\",\"21\",\"28\",\"35\",\"42\",\"49\"\r\n"
+ "\"8\",\"8\",\"16\",\"24\",\"32\",\"40\",\"48\",\"56\"\r\n"
+ "\"9\",\"9\",\"18\",\"27\",\"36\",\"45\",\"54\",\"63\"\r\n"
+ "\"10\",\"10\",\"20\",\"30\",\"40\",\"50\",\"60\",\"70\"\r\n"
+ "\"11\",\"11\",\"22\",\"33\",\"44\",\"55\",\"66\",\"77\"\r\n"
+ "\"12\",\"12\",\"24\",\"36\",\"48\",\"60\",\"72\",\"84\"\r\n"
+ "\"13\",\"13\",\"26\",\"39\",\"52\",\"65\",\"78\",\"91\"\r\n"
+ "\"14\",\"14\",\"28\",\"42\",\"56\",\"70\",\"84\",\"98\"\r\n"
+ "\"15\",\"15\",\"30\",\"45\",\"60\",\"75\",\"90\",\"105\"\r\n"
+ "\"16\",\"16\",\"32\",\"48\",\"64\",\"80\",\"96\",\"112\"\r\n"
+ "\"17\",\"17\",\"34\",\"51\",\"68\",\"85\",\"102\",\"119\"\r\n"
+ "\"18\",\"18\",\"36\",\"54\",\"72\",\"90\",\"108\",\"126\"\r\n"
+ "\"19\",\"19\",\"38\",\"57\",\"76\",\"95\",\"114\",\"133\"\r\n"
+ "\"20\",\"20\",\"40\",\"60\",\"80\",\"100\",\"120\",\"140\"\r\n"
+ "\"21\",\"21\",\"42\",\"63\",\"84\",\"105\",\"126\",\"147\"\r\n"
+ "\"22\",\"22\",\"44\",\"66\",\"88\",\"110\",\"132\",\"154\"\r\n"
+ "\"23\",\"23\",\"46\",\"69\",\"92\",\"115\",\"138\",\"161\"\r\n"
+ "\"24\",\"24\",\"48\",\"72\",\"96\",\"120\",\"144\",\"168\"\r\n"
+ "\"25\",\"25\",\"50\",\"75\",\"100\",\"125\",\"150\",\"175\"\r\n"
+ "\"26\",\"26\",\"52\",\"78\",\"104\",\"130\",\"156\",\"182\"\r\n";

                string output = @"<table>
<tr>
<th></th>
<th>A</th>
<th>B</th>
<th>C</th>
<th>D</th>
<th>E</th>
<th>F</th>
<th>G</th>
</tr>
<tr>
<td>1</td>
<td>1</td>
<td>2</td>
<td>3</td>
<td>4</td>
<td>5</td>
<td>6</td>
<td>7</td>
</tr>
<tr>
<td>2</td>
<td>2</td>
<td>4</td>
<td>6</td>
<td>8</td>
<td>10</td>
<td>12</td>
<td>14</td>
</tr>
<tr>
<td>3</td>
<td>3</td>
<td>6</td>
<td>9</td>
<td>12</td>
<td>15</td>
<td>18</td>
<td>21</td>
</tr>
<tr>
<td>4</td>
<td>4</td>
<td>8</td>
<td>12</td>
<td>16</td>
<td>20</td>
<td>24</td>
<td>28</td>
</tr>
<tr>
<td>5</td>
<td>5</td>
<td>10</td>
<td>15</td>
<td>20</td>
<td>25</td>
<td>30</td>
<td>35</td>
</tr>
<tr>
<td>6</td>
<td>6</td>
<td>12</td>
<td>18</td>
<td>24</td>
<td>30</td>
<td>36</td>
<td>42</td>
</tr>
<tr>
<td>7</td>
<td>7</td>
<td>14</td>
<td>21</td>
<td>28</td>
<td>35</td>
<td>42</td>
<td>49</td>
</tr>
<tr>
<td>8</td>
<td>8</td>
<td>16</td>
<td>24</td>
<td>32</td>
<td>40</td>
<td>48</td>
<td>56</td>
</tr>
<tr>
<td>9</td>
<td>9</td>
<td>18</td>
<td>27</td>
<td>36</td>
<td>45</td>
<td>54</td>
<td>63</td>
</tr>
<tr>
<td>10</td>
<td>10</td>
<td>20</td>
<td>30</td>
<td>40</td>
<td>50</td>
<td>60</td>
<td>70</td>
</tr>
<tr>
<td>11</td>
<td>11</td>
<td>22</td>
<td>33</td>
<td>44</td>
<td>55</td>
<td>66</td>
<td>77</td>
</tr>
<tr>
<td>12</td>
<td>12</td>
<td>24</td>
<td>36</td>
<td>48</td>
<td>60</td>
<td>72</td>
<td>84</td>
</tr>
<tr>
<td>13</td>
<td>13</td>
<td>26</td>
<td>39</td>
<td>52</td>
<td>65</td>
<td>78</td>
<td>91</td>
</tr>
<tr>
<td>14</td>
<td>14</td>
<td>28</td>
<td>42</td>
<td>56</td>
<td>70</td>
<td>84</td>
<td>98</td>
</tr>
<tr>
<td>15</td>
<td>15</td>
<td>30</td>
<td>45</td>
<td>60</td>
<td>75</td>
<td>90</td>
<td>105</td>
</tr>
<tr>
<td>16</td>
<td>16</td>
<td>32</td>
<td>48</td>
<td>64</td>
<td>80</td>
<td>96</td>
<td>112</td>
</tr>
<tr>
<td>17</td>
<td>17</td>
<td>34</td>
<td>51</td>
<td>68</td>
<td>85</td>
<td>102</td>
<td>119</td>
</tr>
<tr>
<td>18</td>
<td>18</td>
<td>36</td>
<td>54</td>
<td>72</td>
<td>90</td>
<td>108</td>
<td>126</td>
</tr>
<tr>
<td>19</td>
<td>19</td>
<td>38</td>
<td>57</td>
<td>76</td>
<td>95</td>
<td>114</td>
<td>133</td>
</tr>
<tr>
<td>20</td>
<td>20</td>
<td>40</td>
<td>60</td>
<td>80</td>
<td>100</td>
<td>120</td>
<td>140</td>
</tr>
<tr>
<td>21</td>
<td>21</td>
<td>42</td>
<td>63</td>
<td>84</td>
<td>105</td>
<td>126</td>
<td>147</td>
</tr>
<tr>
<td>22</td>
<td>22</td>
<td>44</td>
<td>66</td>
<td>88</td>
<td>110</td>
<td>132</td>
<td>154</td>
</tr>
<tr>
<td>23</td>
<td>23</td>
<td>46</td>
<td>69</td>
<td>92</td>
<td>115</td>
<td>138</td>
<td>161</td>
</tr>
<tr>
<td>24</td>
<td>24</td>
<td>48</td>
<td>72</td>
<td>96</td>
<td>120</td>
<td>144</td>
<td>168</td>
</tr>
<tr>
<td>25</td>
<td>25</td>
<td>50</td>
<td>75</td>
<td>100</td>
<td>125</td>
<td>150</td>
<td>175</td>
</tr>
<tr>
<td>26</td>
<td>26</td>
<td>52</td>
<td>78</td>
<td>104</td>
<td>130</td>
<td>156</td>
<td>182</td>
</tr>
</table>";

                #endregion
                IDocument document = Substitute.For<IDocument>();

                MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(input));
                document.GetStream().Returns(stream);
                document.Content.Returns(input);
                IExecutionContext context = Substitute.For<IExecutionContext>();

                CsvToHtml module = new CsvToHtml().WithHeader();

                // When
                module.Execute(new[] { document }, context).ToList();  // Make sure to materialize the result list

                // Then
                context.Received().GetDocument(document, output);
                stream.Dispose();
            }
        }
    }
}
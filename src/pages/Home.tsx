import { Link } from 'react-router-dom'
// components
import {
  AvatarList,
  CardComponents,
  CustomChart,
  CustomTable,
  Header,
  StyledH2,
  StyledH3,
  Styledspan,
} from '@/components'
import { Container, Grid } from '@mui/material'

// hooks
import { useGet } from '@/hooks'

// utlis
import { currencyConverter, highlightTextConvert } from '@/utils'

// types
import { HighlightsData, StarsData, NewsData, CustomChartProps } from '@/types'
//import { HightLightsData } from "@/types";

function Home() {
  // eslint-disable-next-line @typescript-eslint/no-unused-vars
  const {
    data: highlightsData,
    loading: highlightsLoading,
    error: highlightsError,
  } = useGet<HighlightsData[]>('sales/highlights')

  const {
    data: salesMonthData,
    loading: salesMonthLoading,
    error: salesMonthError,
  } = useGet<CustomChartProps>('sales/month')

  const {
    data: salesStarsData,
    loading: salesStarsLoading,
    error: salesStarsError,
  } = useGet<StarsData[]>('sales/stars')

  const {
    data: newData,
    loading: newsLoading,
    error: newsError,
  } = useGet<NewsData[]>('news')

  const {
    data: salesYearData,
    loading: salesYearLoading,
    error: salesYearError,
  } = useGet<CustomChartProps>('sales/year')

  return (
    <>
      <Header />
      <Container className="mb-2" maxWidth="lg">
        <Grid container spacing={4}>
          {!highlightsError && (
            <>
              <Grid item xs={12} md={4}>
                <CardComponents
                  className={
                    highlightsLoading
                      ? 'skeleton-loading skeleton-loading-mh-1'
                      : ''
                  }
                >
                  {!highlightsLoading && highlightsData && (
                    <>
                      <StyledH2 className="mb-1">
                        Total de vendas no mes
                      </StyledH2>
                      <StyledH3>
                        {currencyConverter(highlightsData[0].value)}
                      </StyledH3>
                      <Styledspan>{highlightsData[0].subtitle}</Styledspan>
                    </>
                  )}
                </CardComponents>
              </Grid>
              <Grid item xs={12} md={4}>
                <CardComponents
                  className={
                    highlightsData
                      ? highlightsData[1].subtitle
                      : 'skeleton-loading skeleton-loading-mh-1'
                  }
                >
                  {!highlightsLoading && highlightsData && (
                    <>
                      <StyledH2 className="mb-1">Meta do mes</StyledH2>
                      <StyledH3
                        className="mb-1"
                        sizer={40}
                        lineheight={40}
                        color="white"
                      >
                        {currencyConverter(highlightsData[1].value)}
                      </StyledH3>
                      <Styledspan>
                        {highlightTextConvert(highlightsData[1].subtitle)}
                      </Styledspan>
                    </>
                  )}
                </CardComponents>
              </Grid>

              <Grid item xs={12} md={4}>
                <CardComponents
                  className={
                    highlightsLoading
                      ? 'skeleton-loading skeleton-loading-mh-1'
                      : ''
                  }
                >
                  {!highlightsLoading && highlightsData && (
                
                      <Link to="/leads">
                        <StyledH2 className="mb-1">Leads contactados</StyledH2>
                        <StyledH3>
                          {currencyConverter(highlightsData[2].value)}
                        </StyledH3>
                        <Styledspan>{highlightsData[2].subtitle}</Styledspan>
                      </Link>
           
                  )}
                </CardComponents>
              </Grid>
            </>
          )}

          <Grid item xs={12} md={7}>
            {!salesMonthError && (
              <CardComponents
                className={
                  salesMonthLoading
                    ? 'skeleton-loading skeleton-loading-mh-2'
                    : ''
                }
              >
                {!salesMonthLoading && salesMonthData && (
                  <>
                    <StyledH2 className="mb-1">Valor de venda no mes</StyledH2>
                    <CustomChart
                      labels={salesMonthData.labels.map((label) => label)}
                      data={salesMonthData.data.map((data) => data)}
                      type={salesMonthData.type}
                    />
                  </>
                )}
              </CardComponents>
            )}
          </Grid>

          <Grid item xs={12} md={5}>
            {!salesStarsError && (
              <CardComponents
                className={
                  salesStarsLoading
                    ? 'skeleton-loading skeleton-loading-mh-2'
                    : ''
                }
              >
                {!salesStarsLoading && salesStarsData && (
                  <>
                    <StyledH2 className="mb-1">
                      Maiores vendedores no mes
                    </StyledH2>
                    <AvatarList
                      listData={salesStarsData.map((star) => ({
                        avatar: '/Avatar.svg',
                        name: star.name,
                        subtitle: currencyConverter(star.value),
                      }))}
                    />
                  </>
                )}
              </CardComponents>
            )}
          </Grid>

          <Grid item xs={12} md={5}>
            {!newsError && (
              <CardComponents
                className={
                  newsLoading ? 'skeleton-loading skeleton-loading-mh-2' : ''
                }
              >
                {!newsLoading && newData && (
                  <>
                    <StyledH2 className="mb-1">Noticias relevantes</StyledH2>
                    <CustomTable
                      headers={['Titulo', 'Horario']}
                      rows={newData.map((news) => [
                        <a
                          className="ellipsis ellipsis-sm"
                          href={news.link}
                          target="_blank"
                        >
                          {news.title}
                        </a>,
                        <a href={news.link} target="_blank">
                          {news.date}
                        </a>,
                      ])}
                    />
                  </>
                )}
              </CardComponents>
            )}
          </Grid>

          <Grid item xs={12} md={7}>
            {!salesYearError && (
              <CardComponents
                className={
                  salesYearLoading
                    ? 'skeleton-loading skeleton-loading-mh-2'
                    : ''
                }
              >
                {!salesYearLoading && salesYearData && (
                  <>
                    <StyledH2 className="mb-1">Valor de venda por mes</StyledH2>
                    <CustomChart
                      labels={salesYearData.labels.map((label) => label)}
                      data={salesYearData.data.map((data) => data)}
                      type={salesYearData.type}
                    />
                  </>
                )}
              </CardComponents>
            )}
          </Grid>
        </Grid>
      </Container>
    </>
  )
}
export default Home

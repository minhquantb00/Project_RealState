<script setup>
import axios from '@axios'
import VueApexCharts from 'vue3-apexcharts'

import { onMounted, ref, watch } from 'vue'

const queryParams = ref({})

const listTeam = ref([])

watch(
  () => queryParams.value.startTime,
  newVal => {
    getSalesStatisticsOfCompany()
    ratioBetweenTeamAndCompany()
  },
)

watch(
  () => queryParams.value.endTime,
  newVal => {
    getSalesStatisticsOfCompany()
    ratioBetweenTeamAndCompany()
  },
)

watch(
  () => queryParams.value.teamId,
  newVal => {
    getSalesStatisticsOfCompany()
    ratioBetweenTeamAndCompany()
  },
)


onMounted(() => {
  getSalesStatisticsOfCompany()
  ratioBetweenTeamAndCompany()
  getAllTeams()
})

const getAllTeams = async () => {
  const res = await axios.get('/team/get-all?pageSize=10&pageNumber=1')

  listTeam.value = res.data.data
}

const getSalesStatisticsOfCompany = async () => {
  const res = await axios.post(`/product/TeamStatistic`, { ...queryParams.value })

  chartOptions.value.xaxis.categories = []

  series.value = [{
    name: 'Lợi nhuận',
    data: [],
  }, {
    name: 'Doanh số',
    data: [],
  }]

  res.data.forEach(element => {
    chartOptions.value.xaxis.categories.push(`${element.teamName}`)
    series.value[0].data.push(element.statisticSalesAndProfit.profit)
    series.value[1].data.push(element.statisticSalesAndProfit.sales)
  })

  chartOptions.value = { ...chartOptions.value }
} 

const ratioBetweenTeamAndCompany = async () => {
  const res = await axios.post(`/product/RatioBetweenTeamAndCompany`, { ...queryParams.value })

  doanhSoRatioChartOptions.value.labels.length = []
  loiNhuanRatioChartOptions.value.labels.length = []

  doanhSoRatioSeries.value.length = 0
  loiNhuanRatioSeries.value.length = 0


  res.data.forEach( element => {
    doanhSoRatioSeries.value.push(element.ratioSales)
    doanhSoRatioChartOptions.value.labels.push(`${element.teamName}`)

    loiNhuanRatioSeries.value.push(element.ratioProfit)
    loiNhuanRatioChartOptions.value.labels.push(`${element.teamName}`)

  })

  doanhSoRatioChartOptions.value = { ...doanhSoRatioChartOptions.value }
  loiNhuanRatioChartOptions.value = { ...loiNhuanRatioChartOptions.value }
}

const chartOptions = ref({
  chart: {
    type: 'bar',
    height: 350,
  },
  plotOptions: {
    bar: {
      horizontal: false,
      columnWidth: '55%',
      endingShape: 'rounded',
    },
  },
  dataLabels: {
    enabled: false,
  },
  stroke: {
    show: true,
    width: 2,
    colors: ['transparent'],
  },
  xaxis: {
    categories: [],
  },
  yaxis: {
    title: {
      text: 'VNĐ',
    },
  },
  fill: {
    opacity: 1,
  },
  tooltip: {
    y: {
      formatter: function (val) {
        return "VNĐ " + val + " Việt Nam Đồng"
      },
    },
  },
})

const series = ref([{
  name: 'Profit',
  data: [],
}, {
  name: 'Sales',
  data: [],
}])

const doanhSoRatioSeries = ref([])

const doanhSoRatioChartOptions = ref({
  chart: {
    type: 'pie',
  },
  stroke: {
    colors: ['#fff'],
  },
  fill: {
    opacity: 0.8,
  },
  labels: [],
  responsive: [{
    breakpoint: 480,
    options: {
      chart: {
        width: 200,
      },
      legend: {
        position: 'bottom',
      },
    },
  }],
})

const loiNhuanRatioSeries = ref([])

const loiNhuanRatioChartOptions = ref({
  chart: {
    type: 'pie',
  },
  stroke: {
    colors: ['#fff'],
  },
  fill: {
    opacity: 0.8,
  },
  labels: [],
  responsive: [{
    breakpoint: 480,
    options: {
      chart: {
        width: 200,
      },
      legend: {
        position: 'bottom',
      },
    },
  }],
})
</script>

<template>
  <div>
    <VCard style="padding: 20px;">
      <VRow class="mb-5">
        <VCol cols="3">
          <AppDateTimePicker
            v-model="queryParams.startTime"
            clearable
            placeholder="Thời gian từ"
          />
        </VCol>
        <VCol cols="3">
          <AppDateTimePicker
            v-model="queryParams.endTime"
            clearable
            placeholder="Thời gian đến"
          />
        </VCol>
        
        <VCol cols="3">
          <VSelect
            v-model="queryParams.teamId"
            clearable
            label="Phòng ban"
            :items="listTeam"
            item-value="id"
            item-title="name"
          />
        </VCol>
      </VRow>
      <VCardTitle />
      <VCardText>
        <VRow>
          <VCol cols="12">
            <h2>Doanh số, lợi nhuận theo phòng ban</h2>
            <VueApexCharts
              type="bar"
              height="400"
              :options="chartOptions"
              :series="series"
            />
          </VCol>
          <VCol
            cols="6"
            style="padding: 40px;"
          >
            <h2>Tỉ lệ doanh số các phòng ban</h2>
            <VueApexCharts
              type="polarArea"
              :options="doanhSoRatioChartOptions"
              :series="doanhSoRatioSeries"
            />
          </VCol>
          <VCol
            cols="6"
            style="padding: 40px;"
          >
            <h2>Tỉ lệ lợi nhuận các phòng ban</h2>
            
            <VueApexCharts
              type="polarArea"
              :options="loiNhuanRatioChartOptions"
              :series="loiNhuanRatioSeries"
            />
          </VCol>
        </VRow>
      </VCardText>
    </VCard>
  </div>
</template>

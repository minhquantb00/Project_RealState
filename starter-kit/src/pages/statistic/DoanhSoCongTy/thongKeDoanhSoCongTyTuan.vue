<script setup>
import axios from '@axios'
import VueApexCharts from 'vue3-apexcharts'

import { onMounted, ref } from 'vue'

onMounted(() => {
  getSalesStatisticsOfCompany()
})

const getSalesStatisticsOfCompany = async () => {
  const res = await axios.get('/product/WeeklySalesStatisticsOfCompany')

  chartOptions.value.xaxis.categories = []

  series.value = [{
    name: 'Lợi nhuận',
    data: [],
  }, {
    name: 'Doanh số',
    data: [],
  }]

  res.data.forEach(element => {
    chartOptions.value.xaxis.categories.push(`${element.weekNumber}`)
    series.value[0].data.push(element.profit)
    series.value[1].data.push(element.sales)
  })

  chartOptions.value = { ...chartOptions.value }
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
</script>

<template>
  <div>
    <VCard style="padding: 20px;">
      <VCardTitle>Doanh số, lợi nhuận công ty theo tuần</VCardTitle>
      <VCardText>
        <VueApexCharts
          type="bar"
          height="400"
          :options="chartOptions"
          :series="series"
        />
      </VCardText>
    </VCard>
  </div>
</template>

<template>
	<table class="table table-striped">
		<thead>
			<tr>
				<th scope="col">ISIN</th>
				<th scope="col">Name</th>
				<th scope="col">Price</th>
				<th scope="col">Price Change</th>
				<th scope="col">% Price Change</th>
				<th scope="col">Count</th>
				<th scope="col">Sum</th>
				<th scope="col">Sum Change</th>
				<th scope="col">Div/year Sum</th>
				<th scope="col">Div/total Sum</th>
			</tr>
		</thead>
		<tbody>
			<tr v-for="asset in inventory" :key="asset.id">
				<portfolio-asset :asset="asset" />
			</tr>
		</tbody>
	</table>
</template>
<script lang="ts">
import { Options, Vue } from 'vue-class-component';
import PortfolioAsset from '@/component/portfolioAsset.vue';
import { State } from 'vuex-class';
import { VirtualStateDto } from '@/api/market';

@Options({
	name: 'PortfolioView',
	components: {
		PortfolioAsset,
	},
})
export default class PortfolioView extends Vue {
	@State('virtualState')
	virtualState!: VirtualStateDto;

	get inventory() {
		return this.virtualState.inventory;
	}
}
</script>

<template>
	<div>
		<h2>Brokers</h2>
		<div v-for="broker in brokers" :key="broker.id">
			<broker :broker="broker" />
		</div>
		<router-link to="/addBroker" class="btn btn-primary"
			>Add Broker</router-link
		>
	</div>
</template>
<script lang="ts">
import { Options, Vue } from 'vue-class-component';
import Broker from '@/component/broker.vue';
import { StateDto } from '@/api/state';
import { State } from 'vuex-class';

@Options({
	name: 'BrokerView',
	components: {
		Broker,
	},
})
export default class BrokerView extends Vue {
	@State('activeState')
	activeState!: StateDto;

	get brokers() {
		return this.activeState.brokers;
	}
}
</script>
